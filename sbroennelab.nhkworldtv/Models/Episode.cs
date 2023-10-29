
namespace sbroennelab.nhkworldtv
{

    public class Episode
    {
        public string pgm_gr_id { get; set; }
        public string pgm_id { get; set; }
        public string pgm_no { get; set; }
        public string image { get; set; }
        public string image_l { get; set; }
        public string image_promo { get; set; }
        public bool ignore_domestic { get; set; }
        public object nod_url { get; set; }
        public string voice_lang { get; set; }
        public string[] caption_langs { get; set; }
        public string[] voice_langs { get; set; }
        public string vod_id { get; set; }
        public long onair { get; set; }
        public long vod_to { get; set; }
        public string movie_lengh { get; set; }
        public int movie_duration { get; set; }
        public string analytics { get; set; }
        public string title { get; set; }
        public string title_clean { get; set; }
        public string sub_title { get; set; }
        public string sub_title_clean { get; set; }
        public string description { get; set; }
        public string description_clean { get; set; }
        public string url { get; set; }
        public int[] category { get; set; }
        public int? mostwatch_ranking { get; set; }
        public object[] related_episodes { get; set; }
        public string[] tags { get; set; }
        public object[] chapter_list { get; set; }
        public object transcript_path { get; set; }
        public int life { get; set; }
        public object[] life_category { get; set; }
        public object[] promotion { get; set; }
    }
}