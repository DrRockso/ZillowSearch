using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZillowWebApp.Models
{
    public class Home
    {
        public string SearchAddress { get; set; }
        public string SearchCity { get; set; }
        public string SearchState { get; set; }
        public string SearchZip { get; set; }
        public bool SearchZestimate { get; set; }

        public SearchResult Result { get; set; }
        public List<ErrorResult> Errors { get; set; }

        public Home()
        {
            Result = new SearchResult();
            Errors = new List<ErrorResult>();
        }

        public class ErrorResult
        {
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class SearchResult
        {
            public string ResultStreet { get; set; }
            public string ResultZip { get; set; }
            public string ResultCity { get; set; }
            public string ResultState { get; set; }
            public string ResultLat { get; set; }
            public string ResultLong { get; set; }
            public Zestimate ResultZest { get; set; }

            public SearchResult()
            {
                ResultZest = new Zestimate();
            }            

            public class Zestimate
            {
                public string Currency { get; set; }
                public int Amount { get; set; }
                public string Updated { get; set; }
                public bool OneWeekChange { get; set; }
                public ValueChange Changed { get; set; }
                public ValuationRange Range { get; set; }

                public Zestimate()
                {
                    Changed = new ValueChange();
                    Range = new ValuationRange();
                }

                public class ValueChange
                {
                    public int Amount { get; set; }
                    public int Duration { get; set; }

                }

                public class ValuationRange
                {
                    public int Low { get; set; }
                    public int High { get; set; }
                }
                
            }
        }
    }
}