using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace rrmp_to_pte
{
    class Program
    {
        static IWebElement PollForElement(IWebDriver wd, By by, TimeSpan? maxWait = null)
        {
            maxWait = maxWait ?? TimeSpan.FromSeconds(10);
            var startedAt = DateTime.Now;
            while (true)
            {
                Console.WriteLine("Polling for element");
                var elementsFound = wd.FindElements(by);
                if (elementsFound.Count > 0)
                {
                    Console.WriteLine("Found the element");
                    return elementsFound[0];
                }
                else
                {
                    if (DateTime.Now.Subtract(startedAt) > maxWait)
                    {
                        throw new Exception($"Max wait exceeded for element.");
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }
            }
        }
        static void Main(string[] args)
        {
            var testingRecipeInfoFile = "C:\\temp\\ri.json";
            RecipeInfo ri;

            var configRoot = (new ConfigurationBuilder()).AddUserSecrets<Program>().Build();

            Console.WriteLine("Start");
            string pteBaseUrl = "https://www.plantoeat.com";
            string rrmpBaseUrl = "https://meals.richroll.com";

            if (File.Exists(testingRecipeInfoFile) == false)
            {
                string recipeUrl = (args.Length > 0 ? args[0] : rrmpBaseUrl + "/recipe/56c496ebf83f55a2e4d91af9?ref=discover");
                ri = new RecipeInfo()
                {
                    RecipeUrl = recipeUrl
                };

                Console.WriteLine($"Recipe URL: {{recipeUrl}}");

                Console.WriteLine("Getting the info from the Rich Roll Meal Planner");
                using (IWebDriver wd = new ChromeDriver())
                {
                    wd.Manage().Window.Maximize();

                    LoginToRrmp(wd, rrmpBaseUrl, configRoot);

                    wd.Navigate().GoToUrl(recipeUrl);

                    var titleElement = PollForElement(wd, By.ClassName("recipe-public__title"));
                    ri.Title = titleElement.Text;
                    ri.ImportDirections(wd.FindElement(By.ClassName("recipe__instructions")).Text);
                    var ingredientsElement = wd.FindElement(By.ClassName("ingredients__list-main"));
                    ri.ImportIngredients(ingredientsElement.Text);
                    var imgElement = wd.FindElement(By.ClassName("recipe-public__intro-img"));
                    Console.WriteLine("Title:" + ri.Title);
                    Console.WriteLine("Directions:" + ri.Directions);

                    ri.Description = wd.FindElement(By.ClassName("recipe-font-definition-overrides")).Text;

                    String imageSrc = imgElement.GetCssValue("background-image");
                    Console.WriteLine("Image Src:" + imageSrc);
                    ri.GetImage(imageSrc, Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), @"Pictures\Recipes"));

                    var servingsElement = wd.FindElement(By.ClassName("servings-control__servings"));
                    ri.ImportServings(servingsElement.Text);

                    var nutritionAnchor = wd.FindElement(By.LinkText("NUTRITION"));
                    nutritionAnchor.Click();

                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    var caloriesElements = wd.FindElements(By.ClassName("nutrient-header"));
                    ri.Calories = caloriesElements[1].Text;


                    //  String logoSRC = imgElement.GetCssValue("background-image");

                    //  URL imageURL = new Uri(logoSRC);
                    //  BufferedImage saveImage = ImageIO.read(imageURL);

                    //  ImageIO.write(saveImage, "png", new File("logo-image.png"));

                    // }
                    // catch
                    // {
                    //     throw;
                    // }
                }

                // Console.WriteLine("Writing recipe info to the file");
                // File.WriteAllText(testingRecipeInfoFile, JsonConvert.SerializeObject(ri), Encoding.Default);
            }
            else
            {
                var reicpeInfoText = File.ReadAllText(testingRecipeInfoFile);
                ri = JsonConvert.DeserializeObject<RecipeInfo>(reicpeInfoText);
            }

            Console.WriteLine("Starting the add recipe to Plan to Eat");
            using (IWebDriver wd = new ChromeDriver())
            {
                wd.Manage().Window.Maximize();
                Console.WriteLine("Logging into PTE");
                wd.Navigate().GoToUrl(pteBaseUrl + "/login/");
                wd.FindElement(By.Id("user_login"))
                    .SendKeys(configRoot["PlanToEatUserID"]);

                wd.FindElement(By.Id("user_password"))
                    .SendKeys(configRoot["PlanToEatPassword"]);
                wd.FindElement(By.ClassName("w100")).Click();

                PollForElement(wd, By.ClassName("add-recipe")).Click();

                //Wait for the new recipe modal
                PollForElement(wd, By.ClassName("close-modal"));

                wd.FindElement(By.ClassName("add_change_photo")).Click();

                wd.SwitchTo().Frame("addChangePhotoIframe"); //Photo info is in an iFrame

                PollForElement(wd, By.Id("photo_photo_url")).SendKeys(ri.WebImageFile);

                wd.SwitchTo().DefaultContent(); //Back to the main content

                Thread.Sleep(TimeSpan.FromSeconds(1));

                //the first is the main save in the background, second is the image
                wd.FindElements(By.XPath("//button[text()[contains(., 'Save')]]"))
                                    .ToList().Where(n => n.Displayed && n.Enabled).ToList()
                                    .Last().Click();

                Thread.Sleep(TimeSpan.FromSeconds(3));

                wd.FindElement(By.XPath("//input[@name='recipe[title]']"))
                    .SendKeys(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ri.Title.ToLower()));
                wd.FindElement(By.XPath("//input[@name='recipe[source]']")).SendKeys(ri.RecipeUrl);
                if (String.IsNullOrWhiteSpace(ri.Calories) == false)
                {//TODO: Need to fix the import to get the calories
                    wd.FindElement(By.XPath("//input[@name='recipe[calories]']")).SendKeys(ri.Calories);
                }
                wd.FindElement(By.XPath("//textarea[@name='recipe[description]']")).SendKeys(ri.Description);
                wd.FindElement(By.XPath("//textarea[@name='recipe[directions]']")).SendKeys(ri.Directions);

                wd.FindElement(By.ClassName("ingredients-bulk")).Click();
                Thread.Sleep(TimeSpan.FromSeconds(3));

                wd.FindElement(By.XPath("//textarea[@name='recipe[ingredients_bulk]']")).SendKeys(ri.Ingredients);
                wd.FindElement(By.ClassName("ingredients-line-item")).Click();

                Thread.Sleep(TimeSpan.FromSeconds(3));

                wd.FindElement(By.ClassName("select-edit-recipe-category")).Click();

                Thread.Sleep(TimeSpan.FromSeconds(3));

                // wd.FindElement(By.XPath("//input[@name='courses[196][title]']")).Click();
                // Thread.Sleep(TimeSpan.FromSeconds(1));
                wd.FindElement(By.XPath("//div[@id='cat196']"))
                    .FindElement(By.ClassName("checkbox-custom")).Click();
                Thread.Sleep(TimeSpan.FromSeconds(1));

                wd.FindElement(By.XPath("//button[text()[contains(., 'Save Changes')]]")).Click();
                Thread.Sleep(TimeSpan.FromSeconds(1));

                wd.FindElement(By.XPath("//input[@name='recipe[servings]']")).SendKeys(Keys.Backspace + ri.Servings);

                wd.FindElement(By.XPath("//input[@name='recipe[calories]']")).SendKeys(Math.Round(Decimal.Parse(ri.Calories), 0).ToString());

                Thread.Sleep(TimeSpan.FromSeconds(1));
                wd.FindElement(By.XPath("//button[text()[contains(., 'Save')]]")).Click();

                Console.WriteLine("Done");
            }
        }

        private static void LoginToRrmp(IWebDriver wd, string baseUrl, IConfigurationRoot configRoot)
        {
            Console.WriteLine("Logging into the RRMP");
            wd.Navigate().GoToUrl(baseUrl);
            wd.FindElement(By.ClassName("link-login"))
                .Click();

            PollForElement(wd, By.XPath("//input[@type='email']"))
                .SendKeys(configRoot["RichRollMealPlannerUserID"]);

            wd.FindElement(By.XPath("//input[@type='password']"))
                .SendKeys(configRoot["RichRollMealPlannerPassword"]);

            wd.FindElement(By.ClassName("cta1--modal-primary-cta"))
                .Click();

            PollForElement(wd, By.ClassName("search-wrap"));
            Console.WriteLine("Login to the RRMP Complete");
        }
    }
}
