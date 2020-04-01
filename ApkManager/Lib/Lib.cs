namespace ApkManager.Lib
{
    public class WindowPosition
    {
        public double Top { get; set; }
        public double Left { get; set; }
    }

    public class NameFormat
    {
        public string UsePattern { get; set; }
        public bool UseLabel { get; set; }
        public bool UsePackage { get; set; }
        public bool UseVersion { get; set; }
        public bool UseBuild { get; set; }
        public bool UseSuffixEnclosure { get; set; }
        public Separator Separator { get; set; }
    }

    public enum Separator
    {
        Space,
        Strip,
        Underscore
    }
}
