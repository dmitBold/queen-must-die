using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Cards
{
    public class PagesManager : MonoBehaviour
    {

        public List<PageData> AllPages;

        int page_index;
        public PageData GetPage()
        {
            if (page_index >= AllPages.Count)
            {
                return null;
            }

            PageData currentPage = AllPages[page_index];
            page_index++;

            return currentPage;
        }

        public void Start()
        {
            page_index = 0;
        }

    }
}
