// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Rendering.Dummy;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Text;
using osuTK;

namespace osu.Framework.Tests.Text
{
    [TestFixture]
    public class TextBuilderTest
    {
        private const float font_size = 1;

        private static readonly Vector2 spacing = new Vector2(22, 23);

        private static readonly FontUsage normal_font = new FontUsage("Roboto", weight: "Regular", size: font_size);
        private static readonly FontUsage fixed_width_font = new FontUsage("Roboto", weight: "Regular", size: font_size, fixedWidth: true);

        private readonly FontStore fontStore;
        private readonly ITexturedCharacterGlyph glyphA;
        private readonly ITexturedCharacterGlyph glyphB;
        private readonly ITexturedCharacterGlyph glyphM;
        private readonly ITexturedCharacterGlyph glyphIcon;

        public TextBuilderTest()
        {
            fontStore = new FontStore(new DummyRenderer(), useAtlas: false);
            fontStore.AddTextureSource(new GlyphStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(Game).Assembly), @"Resources"), "Fonts/Roboto/Roboto-Regular"));
            fontStore.AddTextureSource(new GlyphStore(new NamespacedResourceStore<byte[]>(new DllResourceStore(typeof(Game).Assembly), @"Resources"), "Fonts/FontAwesome5/FontAwesome-Solid"));

            glyphA = fontStore.Get(null, 'a');
            glyphB = fontStore.Get(null, 'b');
            glyphM = fontStore.Get(null, 'm');
            glyphIcon = fontStore.Get(null, FontAwesome.Solid.Smile.Icon);
        }

        /// <summary>
        /// Tests that the size of a fresh text builder is zero.
        /// </summary>
        [Test]
        public void TestInitialSizeIsZero()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            Assert.That(builder.Bounds, Is.EqualTo(Vector2.Zero));
        }

        /// <summary>
        /// Tests that the first added character is correctly marked as being on a new line.
        /// </summary>
        [Test]
        public void TestFirstCharacterIsOnNewLine()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].OnNewLine, Is.True);
        }

        /// <summary>
        /// Tests that the first added fixed-width character metrics match the glyph's.
        /// </summary>
        [Test]
        public void TestFirstCharacterRectangleIsCorrect()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo(glyphA.XOffset));
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[0].DrawRectangle.Width, Is.EqualTo(glyphA.Width));
            Assert.That(builder.Characters[0].DrawRectangle.Height, Is.EqualTo(glyphA.Height));
        }

        /// <summary>
        /// Tests that the first added character metrics match the glyph's.
        /// </summary>
        [Test]
        public void TestFirstFixedWidthCharacterRectangleIsCorrect()
        {
            var builder = new TextBuilder(fontStore, fixed_width_font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo((glyphM.Width - glyphA.Width) / 2));
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[0].DrawRectangle.Width, Is.EqualTo(glyphA.Width));
            Assert.That(builder.Characters[0].DrawRectangle.Height, Is.EqualTo(glyphA.Height));
        }

        /// <summary>
        /// Tests that the current position is advanced after a character is added.
        /// </summary>
        [Test]
        public void TestCurrentPositionAdvancedAfterCharacter()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");
            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(glyphA.XAdvance + glyphA.GetKerning(glyphA) + glyphA.XOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Width, Is.EqualTo(glyphA.Width));
            Assert.That(builder.Characters[1].DrawRectangle.Height, Is.EqualTo(glyphA.Height));
        }

        /// <summary>
        /// Tests that the current position is advanced after a fixed width character is added.
        /// </summary>
        [Test]
        public void TestCurrentPositionAdvancedAfterFixedWidthCharacter()
        {
            var builder = new TextBuilder(fontStore, fixed_width_font);

            builder.AddText("a");
            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(glyphM.Width + (glyphM.Width - glyphA.Width) / 2));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Width, Is.EqualTo(glyphA.Width));
            Assert.That(builder.Characters[1].DrawRectangle.Height, Is.EqualTo(glyphA.Height));
        }

        /// <summary>
        /// Tests that a new line added to an empty builder always uses the font height.
        /// </summary>
        [Test]
        public void TestNewLineOnEmptyBuilderOffsetsPositionByFontSize()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(font_size + glyphA.YOffset));
        }

        /// <summary>
        /// Tests that a new line added to an empty line always uses the font height.
        /// </summary>
        [Test]
        public void TestNewLineOnEmptyLineOffsetsPositionByFontSize()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddNewLine();
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset + glyphA.YOffset));
        }

        /// <summary>
        /// Tests that a new line added to a builder that is using the font height as size offsets the y-position by the font size and not the glyph size.
        /// </summary>
        [Test]
        public void TestNewLineUsesFontHeightWhenUsingFontHeightAsSize()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");
            builder.AddText("b");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(font_size + glyphA.YOffset));
        }

        /// <summary>
        /// Tests that a new line added to a builder that is not using the font height as size offsets the y-position by the glyph size and not the font size.
        /// </summary>
        [Test]
        public void TestNewLineUsesGlyphHeightWhenNotUsingFontHeightAsSize()
        {
            var builder = new TextBuilder(fontStore, normal_font, useFontSizeAsHeight: false);

            builder.AddText("a");
            builder.AddText("b");
            builder.AddNewLine();
            builder.AddText("a");

            // b is the larger glyph
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(glyphB.Height));
        }

        /// <summary>
        /// Tests that the bounds are set correctly on a builder which has <see cref="TextBuilder.useFontSizeAsHeight"/> disabled.
        /// </summary>
        [Test]
        public void TestBoundsWhenNotUsingFontHeightAsSize()
        {
            var builder = new TextBuilder(fontStore, normal_font, useFontSizeAsHeight: false);

            var glyphQ = fontStore.Get(normal_font.FontName, 'q').AsNonNull();
            var glyphP = fontStore.Get(normal_font.FontName, 'P').AsNonNull();

            builder.AddText("q");
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphQ.XAdvance, glyphQ.Height)));

            builder.AddText("P");
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphQ.XAdvance + glyphP.GetKerning(glyphQ) + glyphP.XAdvance, glyphQ.Height + (getTrimmedBaseline(glyphP) - getTrimmedBaseline(glyphQ)))));

            builder.Reset();
            builder.AddText("P");
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphP.XAdvance, glyphP.Height)));

            builder.AddText("q");
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphP.XAdvance + glyphQ.GetKerning(glyphP) + glyphQ.XAdvance, glyphQ.Height + (getTrimmedBaseline(glyphP) - getTrimmedBaseline(glyphQ)))));
        }

        /// <summary>
        /// Tests that the first added character on a new line is correctly marked as being on a new line.
        /// </summary>
        [Test]
        public void TestFirstCharacterOnNewLineIsOnNewLine()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[1].OnNewLine, Is.True);
        }

        /// <summary>
        /// Tests that no kerning is added for the first character of a new line.
        /// </summary>
        [Test]
        public void TestFirstCharacterOnNewLineHasNoKerning()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(glyphA.XOffset));
        }

        /// <summary>
        /// Tests that a character with a lower baseline moves the previous character down to align with the new character.
        /// </summary>
        [Test]
        public void TestCharacterWithLowerBaselineOffsetsPreviousCharacters()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");

            Assert.That(builder.LineBaseHeight, Is.EqualTo(glyphA.Baseline));

            builder.AddText($"{glyphIcon.Character}");

            Assert.That(builder.LineBaseHeight, Is.EqualTo(glyphIcon.Baseline));
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));
        }

        /// <summary>
        /// Tests that a character with a higher (lesser in value) baseline gets moved down to align with the previous characters.
        /// </summary>
        [Test]
        public void TestCharacterWithHigherBaselineGetsOffset()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText($"{glyphIcon.Character}");

            Assert.That(builder.LineBaseHeight, Is.EqualTo(glyphIcon.Baseline));

            builder.AddText("a");

            Assert.That(builder.LineBaseHeight, Is.EqualTo(glyphIcon.Baseline));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));
        }

        /// <summary>
        /// Tests that baseline alignment adjustments only affect the line the new character was placed on.
        /// </summary>
        [Test]
        public void TestBaselineAdjustmentAffectsRelevantLineOnly()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");
            builder.AddNewLine();

            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));

            builder.AddText($"{glyphIcon.Character}");

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(font_size + glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(font_size + glyphIcon.YOffset));
        }

        /// <summary>
        /// Tests that baseline alignment adjustments are done correctly on a builder which has <see cref="TextBuilder.useFontSizeAsHeight"/> disabled,
        /// and only affect the line the new character was placed on.
        /// </summary>
        [Test]
        public void TestBaselineAdjustmentWhenNotUsingFontSizeAsHeight()
        {
            var builder = new TextBuilder(fontStore, normal_font, useFontSizeAsHeight: false);

            // test baseline adjustment on the first line
            builder.AddText("a");
            builder.AddText($"{glyphIcon.Character}");
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(getTrimmedBaseline(glyphIcon) - getTrimmedBaseline(glyphA)));

            // test baseline adjustment affects relevant line only
            builder.AddNewLine();
            builder.AddText("a");
            builder.AddText($"{glyphIcon.Character}");
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(getTrimmedBaseline(glyphIcon) - getTrimmedBaseline(glyphA)));
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(glyphIcon.Height + (getTrimmedBaseline(glyphIcon) - getTrimmedBaseline(glyphA))));
        }

        /// <summary>
        /// Tests that accessing <see cref="TextBuilder.LineBaseHeight"/> while the builder has multiline text throws.
        /// </summary>
        [Test]
        public void TestLineBaseHeightThrowsOnMultiline()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("b");

            Assert.Throws<InvalidOperationException>(() => _ = builder.LineBaseHeight);
        }

        /// <summary>
        /// Tests that the current position and "line base height" are correctly reset when the first character is removed.
        /// </summary>
        [Test]
        public void TestRemoveFirstCharacterResetsCurrentPositionAndLineBaseHeight()
        {
            var builder = new TextBuilder(fontStore, normal_font, spacing: spacing);

            builder.AddText("a");
            builder.RemoveLastCharacter();

            Assert.That(builder.LineBaseHeight, Is.EqualTo(0f));
            Assert.That(builder.Bounds, Is.EqualTo(Vector2.Zero));

            builder.AddText("a");

            Assert.That(builder.LineBaseHeight, Is.EqualTo(glyphA.Baseline));
            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo(glyphA.XOffset));
        }

        /// <summary>
        /// Tests that the current position is moved backwards and the character is removed when a character is removed.
        /// </summary>
        [Test]
        public void TestRemoveCharacterOnSameLineRemovesCharacter()
        {
            var builder = new TextBuilder(fontStore, normal_font, spacing: spacing);

            builder.AddText("a");
            builder.AddText("a");
            builder.RemoveLastCharacter();

            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphA.XAdvance, font_size)));

            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(glyphA.XAdvance + spacing.X + glyphA.GetKerning(glyphA) + glyphA.XOffset));
        }

        /// <summary>
        /// Tests that the current position is moved to the end of the previous line, and that the character + new line is removed when a character is removed.
        /// </summary>
        [Test]
        public void TestRemoveCharacterOnNewLineRemovesCharacterAndLine()
        {
            var builder = new TextBuilder(fontStore, normal_font, spacing: spacing);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");
            builder.RemoveLastCharacter();

            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphA.XAdvance, font_size)));

            builder.AddText("a");

            Assert.That(builder.Characters[1].DrawRectangle.TopLeft, Is.EqualTo(new Vector2(glyphA.XAdvance + spacing.X + glyphA.GetKerning(glyphA) + glyphA.XOffset, glyphA.YOffset)));
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphA.XAdvance + spacing.X + glyphA.GetKerning(glyphA) + glyphA.XAdvance, font_size)));
        }

        /// <summary>
        /// Tests that removing a character adjusts the baseline of the relevant line.
        /// </summary>
        [Test]
        public void TestRemoveCharacterAdjustsCharactersBaselineOfRelevantLine()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText($"a{glyphIcon.Character}");
            builder.AddNewLine();
            builder.AddText($"a{glyphIcon.Character}");

            builder.RemoveLastCharacter();

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(font_size + glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(font_size + glyphIcon.YOffset));
            Assert.That(builder.Characters[3].DrawRectangle.Top, Is.EqualTo(font_size * 2 + glyphA.YOffset));

            builder.RemoveLastCharacter();

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(font_size + glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(font_size + glyphIcon.YOffset));
        }

        /// <summary>
        /// Tests that removing a character behaves correctly on a builder which has <see cref="TextBuilder.useFontSizeAsHeight"/> disabled.
        /// </summary>
        [Test]
        public void TestRemoveCharacterWhenNotUsingFontSizeAsHeight()
        {
            var builder = new TextBuilder(fontStore, normal_font, useFontSizeAsHeight: false);

            builder.AddText("ab");
            builder.AddNewLine();
            builder.AddText("am");

            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(glyphB.Height + getTrimmedBaseline(glyphM) - getTrimmedBaseline(glyphA)));
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphA.XAdvance + glyphM.GetKerning(glyphA) + glyphM.XAdvance, glyphB.Height + glyphM.Height + getTrimmedBaseline(glyphM) - getTrimmedBaseline(glyphA))));

            // tests that removing a character resets bounds, and the baseline of the relevant line correctly
            builder.RemoveLastCharacter();

            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(glyphB.Height));
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphA.XAdvance + glyphB.GetKerning(glyphA) + glyphB.XAdvance, glyphB.Height + glyphA.Height)));

            // tests that removing a character on a new line resets the current line to the previous line correctly (tests baseline and bounds after adding a new character)
            builder.RemoveLastCharacter();
            builder.AddText("a");
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(getTrimmedBaseline(glyphB) - getTrimmedBaseline(glyphA)));
            Assert.That(builder.Bounds, Is.EqualTo(new Vector2(glyphA.XAdvance + glyphB.GetKerning(glyphA) + glyphB.XAdvance + glyphA.GetKerning(glyphB) + glyphA.XAdvance, glyphB.Height)));
        }

        /// <summary>
        /// Tests that removing a character from a text that still has another character with the same baseline doesn't affect the alignment.
        /// </summary>
        [Test]
        public void TestRemoveSameBaselineCharacterDoesntAffectAlignment()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText($"a{glyphIcon.Character}{glyphIcon.Character}");

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));

            builder.RemoveLastCharacter();

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));
        }

        /// <summary>
        /// Tests that removing the first character of a line doesn't affect the baseline alignment of the line above it.
        /// </summary>
        [Test]
        public void TestRemoveFirstCharacterOnNewlineDoesntAffectLastLineAlignment()
        {
            var builder = new TextBuilder(fontStore, normal_font);

            builder.AddText($"a{glyphIcon.Character}");
            builder.AddNewLine();
            builder.AddText($"{glyphIcon.Character}");
            builder.RemoveLastCharacter();

            Assert.That(builder.Characters[0].DrawRectangle.Top, Is.EqualTo(glyphA.YOffset + (glyphIcon.Baseline - glyphA.Baseline)));
            Assert.That(builder.Characters[1].DrawRectangle.Top, Is.EqualTo(glyphIcon.YOffset));
        }

        /// <summary>
        /// Tests that the custom user-provided spacing is added for a new character/line.
        /// </summary>
        [Test]
        public void TestSpacingAdded()
        {
            var builder = new TextBuilder(fontStore, normal_font, spacing: spacing);

            builder.AddText("a");
            builder.AddText("a");
            builder.AddNewLine();
            builder.AddText("a");

            Assert.That(builder.Characters[0].DrawRectangle.Left, Is.EqualTo(glyphA.XOffset));
            Assert.That(builder.Characters[1].DrawRectangle.Left, Is.EqualTo(glyphA.XAdvance + spacing.X + glyphA.GetKerning(glyphA) + glyphA.XOffset));
            Assert.That(builder.Characters[2].DrawRectangle.Left, Is.EqualTo(glyphA.XOffset));
            Assert.That(builder.Characters[2].DrawRectangle.Top, Is.EqualTo(font_size + spacing.Y + glyphA.YOffset));
        }

        /// <summary>
        /// Tests that glyph lookup falls back to using the same character with no font name.
        /// </summary>
        [Test]
        public void TestSameCharacterFallsBackWithNoFontName()
        {
            var font = new FontUsage("test", size: font_size);
            var nullFont = new FontUsage(null);
            var builder = new TextBuilder(new TestStore(
                new GlyphEntry(font, new TestGlyph('b', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(nullFont, new TestGlyph('a', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(font, new TestGlyph('?', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(nullFont, new TestGlyph('?', 0, 0, 0, 0, 0, 0, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].Character, Is.EqualTo('a'));
        }

        /// <summary>
        /// Tests that glyph lookup falls back to using the fallback character with the provided font name.
        /// </summary>
        [Test]
        public void TestFallBackCharacterFallsBackWithFontName()
        {
            var font = new FontUsage("test", size: font_size);
            var nullFont = new FontUsage(null);
            var builder = new TextBuilder(new TestStore(
                new GlyphEntry(font, new TestGlyph('b', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(nullFont, new TestGlyph('b', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(font, new TestGlyph('?', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(nullFont, new TestGlyph('?', 1, 0, 0, 0, 0, 0, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].Character, Is.EqualTo('?'));
            Assert.That(builder.Characters[0].XOffset, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that glyph lookup falls back to using the fallback character with no font name.
        /// </summary>
        [Test]
        public void TestFallBackCharacterFallsBackWithNoFontName()
        {
            var font = new FontUsage("test", size: font_size);
            var nullFont = new FontUsage(null);
            var builder = new TextBuilder(new TestStore(
                new GlyphEntry(font, new TestGlyph('b', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(nullFont, new TestGlyph('b', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(font, new TestGlyph('b', 0, 0, 0, 0, 0, 0, 0)),
                new GlyphEntry(nullFont, new TestGlyph('?', 1, 0, 0, 0, 0, 0, 0))
            ), font);

            builder.AddText("a");

            Assert.That(builder.Characters[0].Character, Is.EqualTo('?'));
            Assert.That(builder.Characters[0].XOffset, Is.EqualTo(1));
        }

        /// <summary>
        /// Tests that a null glyph is correctly handled.
        /// </summary>
        [Test]
        public void TestFailedCharacterLookup()
        {
            var font = new FontUsage("test", size: font_size);
            var builder = new TextBuilder(new TestStore(), font);

            builder.AddText("a");

            Assert.That(builder.Bounds, Is.EqualTo(Vector2.Zero));
        }

        /// <summary>
        /// Retrieves the baseline of a glyph when <see cref="TextBuilder.useFontSizeAsHeight"/> is turned off.
        /// </summary>
        /// <param name="glyph">The glyph to return the trimmed baseline for.</param>
        private float getTrimmedBaseline(ITexturedCharacterGlyph glyph) => glyph.Baseline - glyph.YOffset;

        private class TestStore : ITexturedGlyphLookupStore
        {
            private readonly GlyphEntry[] glyphs;

            public TestStore(params GlyphEntry[] glyphs)
            {
                this.glyphs = glyphs;
            }

            public ITexturedCharacterGlyph Get(string fontName, char character)
            {
                if (string.IsNullOrEmpty(fontName))
                    return glyphs.FirstOrDefault(g => g.Glyph.Character == character).Glyph;

                return glyphs.FirstOrDefault(g => g.Font.FontName == fontName && g.Glyph.Character == character).Glyph;
            }

            public Task<ITexturedCharacterGlyph> GetAsync(string fontName, char character) => throw new NotImplementedException();
        }

        private readonly struct GlyphEntry
        {
            public readonly FontUsage Font;
            public readonly ITexturedCharacterGlyph Glyph;

            public GlyphEntry(FontUsage font, ITexturedCharacterGlyph glyph)
            {
                Font = font;
                Glyph = glyph;
            }
        }

        private readonly struct TestGlyph : ITexturedCharacterGlyph
        {
            public Texture Texture => new DummyRenderer().CreateTexture(1, 1);
            public float XOffset { get; }
            public float YOffset { get; }
            public float XAdvance { get; }
            public float Width { get; }
            public float Baseline { get; }
            public float Height { get; }
            public char Character { get; }

            private readonly float glyphKerning;

            public TestGlyph(char character, float xOffset, float yOffset, float xAdvance, float width, float baseline, float height, float kerning)
            {
                glyphKerning = kerning;
                Character = character;
                XOffset = xOffset;
                YOffset = yOffset;
                XAdvance = xAdvance;
                Width = width;
                Baseline = baseline;
                Height = height;
            }

            public float GetKerning<T>(T lastGlyph)
                where T : ICharacterGlyph
                => glyphKerning;
        }
    }
}
