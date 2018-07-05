using System.Collections.Generic;
using System.IO;
using Itinero.Navigation.Language;
using Microsoft.Extensions.Configuration;

namespace rideaway_backend.Instance {
    /// <summary>
    /// Static instance of all the language references.
    /// </summary>
    public static class Languages {
        private static Dictionary<string, ILanguageReference> languages;

        /// <summary>
        /// Initialise the language references for the supported languages.
        /// </summary>
        public static void initialize (IConfiguration configuration) {
            languages = new Dictionary<string, ILanguageReference> ();
            languages.Add ("en", new DefaultLanguageReference ());
            languages.Add ("nl", new CsvLanguageReference (new FileInfo (configuration.GetSection("Paths").GetValue<string>("NlTranslations"))));
            languages.Add ("fr", new CsvLanguageReference (new FileInfo (configuration.GetSection("Paths").GetValue<string>("FrTranslations"))));
        }

        /// <summary>
        /// Get a language reference.
        /// </summary>
        /// <param name="key">key of the language reference to get.</param>
        /// <returns>Language reference corresponding to the given key.</returns>
        public static ILanguageReference GetLanguage (string key) {
            return languages[key];
        }

    }
}