using CloudStorageSample.WP8.Resources;

namespace CloudStorageSample.WP8
{
    using CloudStorageSample.WP8.Resources;

    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}