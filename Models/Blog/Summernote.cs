namespace AppMVC.Models
{
    public class Summernote
    {
        public Summernote(string IDEditor, bool LoadLibrary = true)
        {
            this.IDEditor = IDEditor;
            this.LoadLibrary = LoadLibrary;
        }

        public string IDEditor { get; set; }
        public bool LoadLibrary { get; set; }
        public int height { get; set; } = 120;
        public string toolbar { get; set; } = @"
            [
                // [groupName, [list of button]]
                ['style', ['bold', 'italic', 'underline', 'clear']],
                ['font', ['strikethrough', 'superscript', 'subscript']],
                ['fontsize', ['fontsize']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table',['table']],
                ['insert',['picture', 'link','video','elfinder']],
                ['height', ['height']]
            ]
        ";
    }
}