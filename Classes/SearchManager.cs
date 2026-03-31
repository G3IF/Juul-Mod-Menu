using System.Collections.Generic;
using UnityEngine;

namespace Juul
{
    public class SearchManager
    {
        public static bool IsSearching = false;
        public static bool WasSearchingLastFrame = false;
        public static string SearchQuery = "";

        public static void PerformSearch()
        {
            Category searchCat = new Category();
            searchCat.Name = "Search Results";
            searchCat.Buttons = new List<Button>();
            searchCat.Subcategories = new List<Category>();
            
            string query = SearchQuery.ToLower();
            if (!string.IsNullOrEmpty(query) && Buttons.Modules != null)
            {
                foreach (Category module in Buttons.Modules)
                {
                    if (module == Buttons.EnabledCategory) continue;
                    if (module == PlayerMenu.GetPlayersCategory()) continue;
                    if (module.Buttons != null)
                    {
                        foreach (Button b in module.Buttons)
                        {
                            if (b.Name.ToLower().Contains(query))
                            {
                                searchCat.Buttons.Add(b);
                            }
                        }
                    }
                }
            }
            Core.ActiveCategory = searchCat;
            Core.CurrentPage = 0;
        }
    }
}
