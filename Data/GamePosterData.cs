using MTM101BaldAPI.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MyAPI.Data
{
    public class GamePosterData
    {
        /// <summary>
        /// The poster's reference.
        /// </summary>
        public PosterObject poster;

        /// <summary>
        /// All the poster text stored for this poster.
        /// </summary>
        public PosterText[] texts;

        public GamePosterData(params PosterText[] texts)
        {
            this.texts = texts;
        }

        public PosterTextData[] Convert()
        {
            List<PosterTextData> datas = new List<PosterTextData>();
            for (int i = 0; i < texts.Length; i++)
            {
                PosterTextData data = new PosterTextData();
                var text = texts[i];
                data.font = text.font.FontAsset();
                data.fontSize = text.fontSize != -1 ? text.fontSize : Mathf.RoundToInt(text.font.FontSize());
                data.alignment = TextAlignmentOptions.Center;
                data.color = text.color;
                data.style = text.fontStyles;
                data.textKey = text.textKey;
                data.position = text.pos;
                datas.Add(data);
            }
            return datas.ToArray();
        }
    }

    public class PosterDataBuilder
    {
        public List<PosterText> _text = new List<PosterText>();

        public PosterDataBuilder AddText(params PosterText[] texts)
        {
            _text = texts.ToList();
            return this;
        }

        public PosterDataBuilder AddText(string text, Color color, IntVector2 pos, BaldiFonts font = BaldiFonts.ComicSans24, FontStyles fontStyles = FontStyles.Normal, int fontSize = -1)
        {
            _text.Add(new PosterText(text, color, pos, font, fontStyles, fontSize));
            return this;
        }

        public PosterDataBuilder AddText(string text, Color color, BaldiFonts font = BaldiFonts.ComicSans24, FontStyles fontStyles = FontStyles.Normal, int fontSize = -1)
        {
            _text.Add(new PosterText(text, color, new IntVector2(32, 32), font, fontStyles, fontSize));
            return this;
        }

        public PosterDataBuilder AddText(string text, IntVector2 pos, BaldiFonts font = BaldiFonts.ComicSans24, FontStyles fontStyles = FontStyles.Normal, int fontSize = -1)
        {
            _text.Add(new PosterText(text, Color.black, pos, font, fontStyles, fontSize));
            return this;
        }

        public PosterDataBuilder AddText(string text, BaldiFonts font = BaldiFonts.ComicSans24, FontStyles fontStyles = FontStyles.Normal, int fontSize = -1)
        {
            _text.Add(new PosterText(text, Color.black, new IntVector2(32, 32), font, fontStyles, fontSize));
            return this;
        }

        public GamePosterData Build()
        {
            for (int i = 0; i < _text.Count; i++)
            {
                PosterText text = _text[i];
                if (string.IsNullOrEmpty(text.textKey))
                {
                    _text[i] = new PosterText(" ", Color.clear, new IntVector2(32, 32), BaldiFonts.ComicSans24, FontStyles.Normal, 24);
                }
            }

            return new GamePosterData(_text.ToArray());
        }
    }

    public readonly struct PosterText
    {
        public readonly string textKey;
        public readonly Color color;
        public readonly IntVector2 pos;
        public readonly BaldiFonts font;
        public readonly FontStyles fontStyles;
        public readonly int fontSize;

        public PosterText(string textKey, Color color, IntVector2 pos, BaldiFonts font, FontStyles fontStyles, int fontSize)
        {
            this.textKey = textKey;
            this.color = color;
            this.pos = pos;
            this.font = font;
            this.fontStyles = fontStyles;
            this.fontSize = fontSize;
        }
    }
}