
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace rrmp_to_pte
{
    public class RecipeInfo
    {
        public string RecipeUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        private string _directions = null;
        public string Directions
        {
            get
            {
                var tipsIndex = _directions.IndexOf("Kitchen Tips");
                return (tipsIndex >= 0 ? _directions.Substring(0, tipsIndex - 1) : _directions).Trim();
            }
            set
            {
                _directions = value;
            }
        }
        public int Servings { get; set; }
        public string Calories { get; set; }


        private string _ingredients = null;
        public string Ingredients
        {
            get
            {
                return _ingredients.Replace("&amp;", "and");
            }
            set
            {
                _ingredients = value;
            }
        }

        public string LocalImageFile { get; set; }

        public string WebImageFile { get; set; }

        public string ImportDirections(string rawText)
        {
            this.Directions = rawText;
            if (this.Directions.StartsWith("Directions\r\n"))
            {
                this.Directions = this.Directions.Substring("Directions\r\n".Length);
            }
            if (this.Directions.Contains("Would you make this recipe again?"))
            {
                this.Directions = this.Directions.Substring(0, this.Directions.IndexOf("Would you make this recipe again?"));
            }
            return this.Directions;
        }

        public int ImportServings(string servingsText)
        {
            var count = 0;
            foreach (var entryRaw in servingsText.Split(','))
            {
                var entry = entryRaw.Trim();
                entry.Substring(0, entry.IndexOf(" "));
                var num = Convert.ToInt32(entry.Substring(0, entry.IndexOf(" ")).Trim());
                var size = entry.Substring(entry.IndexOf(" ") + 1);
                switch (size)
                {
                    case "medium":
                        count = count + num;
                        break;
                    case "extra large":
                        count = count + (2 * num);
                        break;
                    default:
                        throw new Exception($"Serving size {{size}} was not recognized");
                }
            }
            this.Servings = count;
            return count;
        }

        public void ImportIngredients(string rawText)
        {
            Console.WriteLine("Importing Ingredients", rawText);
            var list = rawText
                .Trim().Replace("Optional", "")
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList().Where(n => String.IsNullOrWhiteSpace(n) == false).ToArray();

            var result = new StringBuilder();
            for (int i = 0; i < list.Length; i = i + 2)
            {
                result.AppendLine(list[i + 1].Trim() + " " + list[i].Trim());
            }

            this.Ingredients = result.ToString();
        }

        public void GetImage(string imageUrl, string storagePath)
        {
            imageUrl = imageUrl.Substring(5, imageUrl.Length - 7);
            this.WebImageFile = imageUrl;
            var tempFile = Path.Combine(storagePath, imageUrl.Substring(imageUrl.LastIndexOf("/") + 1));
            tempFile = Path.ChangeExtension(tempFile, "png");
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            var fileInfo = new FileInfo(tempFile);
            var _httpClient = new HttpClient();
            var response = _httpClient.GetAsync(imageUrl).Result;
            response.EnsureSuccessStatusCode();
            using var ms = response.Content.ReadAsStreamAsync().Result;
            using var fs = File.Create(fileInfo.FullName);
            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(fs);
            this.LocalImageFile = tempFile;
        }
    }
}