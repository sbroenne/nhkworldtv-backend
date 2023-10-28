
namespace sbroennelab.nhkworldtv
{
    public class Stream
    {
        public string response_status { get; set; }
        public int moviesum { get; set; }
        public Meta[] meta { get; set; }
    }

    public class Meta
    {
        public Movie_Url movie_url { get; set; }
        public Movie_Definition movie_definition { get; set; }

    }

    public class Movie_Url
    {
        public string mb_lq { get; set; }
        public string mb_sq { get; set; }
        public string mb_hq { get; set; }
        public string mb_hd { get; set; }
        public string mb_auto { get; set; }
        public string auto_pc { get; set; }
        public string auto_sp { get; set; }
    }

    public class Movie_Definition
    {
        public string mb_lq { get; set; }
        public string mb_sq { get; set; }
        public string mb_hq { get; set; }
        public string mb_hd { get; set; }
        public string mb_auto { get; set; }
        public string auto_pc { get; set; }
        public string auto_sp { get; set; }
    }

}
