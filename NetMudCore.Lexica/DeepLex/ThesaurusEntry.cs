﻿using System;
using System.Collections.Generic;

namespace NetMudCore.Lexica.DeepLex
{
    [Serializable]
    public class ThesaurusEntry
    {
        /// <summary>
        /// Metadata for the api
        /// </summary>
        public ThesaurusMeta meta { get; set; }

        /// <summary>
        /// Homograph
        /// </summary>
        public int hom { get; set; }

        /// <summary>
        /// Headwords
        /// </summary>
        public Headword hwi { get; set; }

        /// <summary>
        /// Alternate headwords
        /// </summary>
        public List<Headword> ahws { get; set; }

        /// <summary>
        /// Variant spellings and pronounciations
        /// </summary>
        public List<Variant> vrs { get; set; }

        /// <summary>
        /// Functional Label, word form
        /// </summary>
        public string fl { get; set; }

        /// <summary>
        /// General labels, usage specifics
        /// </summary>
        public List<string> lbs { get; set; }

        /// <summary>
        /// subject/status labels
        /// </summary>
        public List<string> sls { get; set; }

        /// <summary>
        /// Inflections
        /// </summary>
        public List<Inflection> ins { get; set; }

        /// <summary>
        /// When a headword is a less common spelling of another word with the same meaning, there will be a cognate cross-reference pointing to the headword with the more common spelling.
        /// </summary>
       // public CognateCrossReference cxs { get; set; }

        /// <summary>
        /// The definition section groups together all the sense sequences and verb dividers for a headword or defined run-on phrase.
        /// </summary>
        public List<Definition> def { get; set; }

        /// <summary>
        /// An undefined entry word is derived from or related to the headword, carries a functional label and possibly other information, but does not have any definitions. 
        /// </summary>
        public List<UndefinedRunOns> uros { get; set; }

        /// <summary>
        /// A defined run-on phrase is an expression or phrasal verb formed from the headword. This phrase, its definition section, and other information such as pronunciations, together make up a defined run-on.
        /// </summary>
        public List<DefinedRunOn> dros { get; set; }

        /// <summary>
        /// DIRECTIONAL CROSS-REFERENCE SECTION: Directional cross-references to other entries may be presented after the main definition section. 
        /// </summary>
        public List<string> dxnls { get; set; }

        /// <summary>
        /// In addition to usage notes within definitions, a more extensive usage discussion may be included in the entry. 
        /// </summary>
        public List<UsageSection> usages { get; set; }

        /// <summary>
        /// Extensive discussions of synonyms for the headword may be included in the entry. 
        /// </summary>
        public List<SynonymsSection> syns { get; set; }

        /// <summary>
        /// In addition to the verbal illustrations provided throughout the entry, a larger section of quotations from cited sources is sometimes included.
        /// </summary>
        public List<QuotationsSection> quotes { get; set; }

        /// <summary>
        /// Entries may have illustrations to provide a visual depiction of the headword. All information needed to display an image is contained in art.
        /// </summary>
        public List<Artwork> art { get; set; }

        /// <summary>
        /// A reference from an entry to a table is contained in table.
        /// </summary>
        public Table table { get; set; }

        /// <summary>
        /// An etymology is an explanation of the historical origin of a word. While the etymology contained in an et most typically relates to the headword, it may also explain the origin of a defined run-on phrase or a particular sense.
        /// </summary>
        //public List<Etymology> et { get; set; }

        /// <summary>
        /// The date of the earliest recorded use of a headword in English is captured in date.
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// A short definition provides a highly abridged version of the main definition section, consisting of just the definition text for the first three senses. 
        /// It is not meant to be displayed along with the full entry, but instead as an alternative, shortened preview of the entry content. A short definition is contained in a shortdef.
        /// </summary>
        public List<string> shortdef { get; set; }

        public ThesaurusEntry()
        {
            syns = new List<SynonymsSection>();
            usages = new List<UsageSection>();
            art = new List<Artwork>();
            quotes = new List<QuotationsSection>();
            ahws = new List<Headword>();
            vrs = new List<Variant>();
            lbs = new List<string>();
            sls = new List<string>();
            ins = new List<Inflection>();
            def = new List<Definition>();
            dros = new List<DefinedRunOn>();
            shortdef = new List<string>();
            dxnls = new List<string>();
            uros = new List<UndefinedRunOns>();
            table = new Table();
            //cxs = new CognateCrossReference();
            //et = new List<Etymology>();
        }

    }
}
