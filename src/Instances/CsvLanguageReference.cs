using System.Collections.Generic;
using System.IO;
using Itinero.Navigation.Language;

namespace rideaway_backend.Instance {
    /// <summary>
    /// Language reference used when generating instructions. This class
    /// is used to initialize a language reference from a csv file.
    /// </summary>
    public class CsvLanguageReference : ILanguageReference {
        private Dictionary<string, string> Dictionary;
        private FileInfo FileInfo;

        /// <summary>
        /// Constructor for the language reference.
        /// </summary>
        /// <param name="FileInfo">FileInfo object of the csv file.</param>
        public CsvLanguageReference (FileInfo FileInfo) {
            this.FileInfo = FileInfo;
            initialize ();
        }

        /// <summary>
        /// Initialises the language reference by reading the file and storing the key-value
        /// pairs in a dictionary.
        /// </summary>
        public void initialize () {
            Dictionary = new Dictionary<string, string> ();
            using (var reader = new StreamReader (FileInfo.OpenRead ())) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine ();
                    var translation = line.Split (',');
                    Dictionary.Add (translation[0], translation[1]);
                }
            }
        }

        /// <summary>
        /// Get the translation of the given value.
        /// <summary> 
        /// <param name="value">value to get the translation for.</param>
        public string this [string value] {
            get {
                if (Dictionary.ContainsKey (value)) {
                    return Dictionary[value];
                } else {
                    return value;
                }
            }

        }

    }
}