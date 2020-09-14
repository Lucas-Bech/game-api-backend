using System;
using System.Collections.Generic;
using System.Text;

namespace GameAPILibrary.Resources
{
    public class Review
    {
        private App _app;
        private Reviewer _reviewer;
        private string _rating;
        private string _url;

        public App App { get => _app; set => _app = value; }
        public Reviewer Reviewer { get => _reviewer; set => _reviewer = value; }
        public string Rating { get => _rating; set => _rating = value; }
        public string Url { get => _url; set => _url = value; }

        public Review()
        {

        }
    }
}
