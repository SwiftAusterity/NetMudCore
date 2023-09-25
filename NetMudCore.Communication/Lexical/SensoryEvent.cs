﻿using NetMudCore.DataAccess.Cache;
using NetMudCore.DataStructure.Architectural;
using NetMudCore.DataStructure.Architectural.EntityBase;
using NetMudCore.DataStructure.Linguistic;
using NetMudCore.DataStructure.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMudCore.Communication.Lexical
{
    [Serializable]
    public class SensoryEvent : ISensoryEvent
    {
        /// <summary>
        /// The thing happening
        /// </summary>
        [UIHint("Lexica")]
        public ILexica Event { get; set; }

        /// <summary>
        /// The perceptive strength (higher = easier to see and greater distance noticed)
        /// </summary>
        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Strength", Description = "How easy is this to sense. Stronger means it can be detected more easily and from a greater distance.")]
        public short Strength { get; set; }

        /// <summary>
        /// The type of sense used to detect this
        /// </summary>
        [Display(Name = "Sensory Type", Description = "The type of sense used to 'see' this.")]
        [UIHint("EnumDropDownList")]
        public MessagingType SensoryType { get; set; }

        public SensoryEvent()
        {
            SensoryType = MessagingType.Visible;
            Strength = 30;
        }

        public SensoryEvent(ILexica happening, short strength, MessagingType sensoryType)
        {
            Event = happening;
            Strength = strength;
            SensoryType = sensoryType;
        }

        /// <summary>
        /// Use this to create a "blank" occurrence
        /// </summary>
        public SensoryEvent(MessagingType sensoryType)
        {
            SensoryType = sensoryType;
            Strength = 30;
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(ILexica modifier, bool passthru = false)
        {
            return Event.TryModify(modifier, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(ILexica[] modifier)
        {
            Event.TryModify(modifier);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(IEnumerable<ILexica> modifier)
        {
            Event.TryModify(modifier);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(ISensoryEvent modifier, bool passthru = false)
        {
            return TryModify(modifier.Event, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(ISensoryEvent[] modifier)
        {
            TryModify(modifier.Select(occ => occ.Event));
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(IEnumerable<ISensoryEvent> modifier)
        {
            TryModify(modifier.Select(occ => occ.Event));
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(LexicalType type, GrammaticalType role, string phrase, bool passthru = false)
        {
            return Event.TryModify(type, role, phrase, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(Tuple<LexicalType, GrammaticalType, string> modifier, bool passthru = false)
        {
            return TryModify(modifier.Item1, modifier.Item2, modifier.Item3, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(Tuple<LexicalType, GrammaticalType, string>[] modifier)
        {
            Event.TryModify(modifier);
        }

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="overridingContext">Context to override the lexica with</param>
        /// <param name="anonymize">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        public IEnumerable<ILexicalSentence> Unpack(LexicalContext overridingContext = null, bool anonymize = false)
        {
            List<ILexicalSentence> sentences = new();

            //short circuit empty lexica
            if (string.IsNullOrWhiteSpace(Event?.Phrase))
            {
                return sentences;
            }

            if (overridingContext != null)
            {
                //Sentence must maintain the same language, tense and personage as well as the weight values
                Event.Context.Language = overridingContext.Language;
                Event.Context.Tense = overridingContext.Tense;
                Event.Context.Perspective = overridingContext.Perspective;
                Event.Context.Elegance = overridingContext.Elegance;
                Event.Context.Severity = overridingContext.Severity;
                Event.Context.Quality = overridingContext.Quality;
            }

            //Language rules engine, default to base language if we have an empty language
            if (Event.Context.Language == null || (Event.Context.Language?.WordPairRules?.Count == 0 && Event.Context.Language?.WordRules?.Count == 0))
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                Event.Context.Language = globalConfig.BaseLanguage;
            }

            if (anonymize)
            {
                LexicalContext pronounContext = Event.Context.Clone();
                pronounContext.Perspective = NarrativePerspective.SecondPerson;
                pronounContext.Position = LexicalPosition.None;
                pronounContext.Tense = LexicalTense.None;
                pronounContext.Determinant = false;
                pronounContext.Semantics = new HashSet<string>();

                IDictata pronoun = Thesaurus.GetWord(pronounContext, LexicalType.Pronoun);
                Event.Phrase = pronoun.Name;
                Event.Type = LexicalType.Pronoun;
            }

            List<ILexica> subjects = new()
            {
                Event
            };
            subjects.AddRange(Event.Modifiers.Where(mod => mod != null && mod.Role == GrammaticalType.Subject));

            Event.Modifiers.RemoveWhere(mod => mod == null || mod.Role == GrammaticalType.Subject);

            foreach (ILexica subject in subjects)
            {
                //This is to catch directly described entities, we have to add a verb to it for it to make sense. "Complete sentence rule"
                if (subject.Modifiers.Any() && !subject.Modifiers.Any(mod => mod.Role == GrammaticalType.Verb))
                {
                    LexicalContext verbContext = subject.Context.Clone();
                    verbContext.Semantics = new HashSet<string> { "existential" };
                    verbContext.Determinant = false;
                    IDictata verb = Thesaurus.GetWord(verbContext, LexicalType.Verb);

                    ILexica verbLex = verb.GetLexica(GrammaticalType.Verb, verbContext);
                    verbLex.TryModify(subject.Modifiers);

                    subject.Modifiers = new HashSet<ILexica>();
                    subject.TryModify(verbLex);
                }

                if (subject.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                {
                    sentences.Add(subject.MakeSentence(SentenceType.Partial, SensoryType, Strength));

                    //fragment sentences
                    foreach (ILexica subLex in subject.Modifiers.Where(mod => mod.Role == GrammaticalType.Subject))
                    {
                        sentences.Add(subLex.MakeSentence(SentenceType.Statement, SensoryType, Strength));
                    }
                }
                else
                {
                    //full obfuscation happens at 100 only
                    if (Strength <= -100 || Strength >= 100)
                    {
                        ILexica lex = RunObscura(SensoryType, subject, subject.Context.Observer, Strength > 0);

                        sentences.Add(lex.MakeSentence(SentenceType.Statement, SensoryType, Strength));
                    }
                    else
                    {
                        sentences.Add(subject.MakeSentence(SentenceType.Statement, SensoryType, Strength));
                    }
                }
            }

            return sentences;
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <returns>a sentence fragment</returns>
        public string Describe()
        {
            return Event.Describe();
        }

        private static ILexica RunObscura(MessagingType sensoryType, ILexica subject, IEntity observer, bool over)
        {
            LexicalContext context = new(observer, null, null)
            {
                Determinant = true,
                Perspective = NarrativePerspective.FirstPerson,
                Position = LexicalPosition.Around
            };

            subject.Modifiers = new HashSet<ILexica>();
            subject.Type = LexicalType.Verb;
            subject.Role = GrammaticalType.Verb;
            subject.Context = context;

            switch (sensoryType)
            {
                case MessagingType.Audible:
                    subject.Phrase = "hear";

                    if (!over)
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "sounds")
                                .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "soft");

                    }
                    else
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "sounds")
                                .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "loud");
                    }
                    break;
                case MessagingType.Olefactory:
                    subject.Phrase = "smell";
                    if (!over)
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "something")
                                                        .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "subtle");

                    }
                    else
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "something")
                                                        .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "pungent");
                    }
                    break;
                case MessagingType.Psychic:
                    subject.Phrase = "sense";
                    if (!over)
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "presence")
                                                        .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "vague");

                    }
                    else
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "presence")
                                                        .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "disturbing");
                    }
                    break;
                case MessagingType.Tactile:
                    subject.Phrase = "brushes";
                    context.Position = LexicalPosition.Attached;
                    if (!over)
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "skin")
                                                        .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "lightly");

                    }
                    else
                    {
                        subject.Phrase = "rubs";
                        context.Elegance = -5;
                        subject.TryModify(LexicalType.Pronoun, GrammaticalType.Subject, "you");
                    }
                    break;
                case MessagingType.Taste:
                    subject.Phrase = "taste";
                    context.Position = LexicalPosition.InsideOf;

                    if (!over)
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "something")
                                                        .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "subtle");

                    }
                    else
                    {
                        context.Elegance = -5;
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "something")
                                                        .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "offensive");
                    }
                    break;
                case MessagingType.Visible:
                    subject.Phrase = "see";
                    context.Plural = true;

                    if (!over)
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "shadows");
                    }
                    else
                    {
                        subject.TryModify(LexicalType.Noun, GrammaticalType.Subject, "lights")
                                                .TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, "blinding");
                    }
                    break;
            }

            return subject;
        }
    }
}
