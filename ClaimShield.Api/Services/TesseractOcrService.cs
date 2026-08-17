using System.Text.RegularExpressions;

using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Ocr;

using Tesseract;

namespace ClaimShield.Api.Services
{
    // =================================================================
    // Real local OCR via the open-source Tesseract engine (no paid API,
    // per the Phase 12 locked architecture) - verified working
    // standalone before this service was written (native win-x64
    // binaries load correctly, eng.traineddata present under
    // ClaimShield.Api/tessdata/).
    //
    // Registration-number extraction is regex-based (fairly reliable,
    // fixed Indian plate format). Owner-name/chassis-number extraction
    // are best-effort line heuristics over free-form RC layouts - there
    // is no structured RC template to key off, so these two fields are
    // genuinely lower-confidence than the registration number by
    // design, not by omission.
    // =================================================================

    public class TesseractOcrService : IOcrService
    {
        private static readonly Regex RegNumberPattern =
            new(
                @"[A-Z]{2}[\s-]?[0-9]{1,2}[\s-]?[A-Z]{1,3}[\s-]?[0-9]{4}",
                RegexOptions.Compiled);

        private static readonly Regex ChassisPattern =
            new(
                @"\b[A-HJ-NPR-Z0-9]{11,17}\b",
                RegexOptions.Compiled);

        private readonly string _tessDataPath;

        public TesseractOcrService(
            IWebHostEnvironment environment)
        {
            _tessDataPath =
                Path.Combine(environment.ContentRootPath, "tessdata");
        }

        public Task<OcrExtractionResult> ExtractAsync(
            byte[] imageBytes)
        {
            return Task.Run(() =>
            {
                using var engine =
                    new TesseractEngine(
                        _tessDataPath,
                        "eng",
                        EngineMode.Default);

                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = engine.Process(img);

                var rawText = page.GetText();
                var confidence = (decimal)page.GetMeanConfidence();

                return new OcrExtractionResult
                {
                    RawText = rawText,
                    RegistrationNumber = ExtractRegistrationNumber(rawText),
                    OwnerName = ExtractOwnerName(rawText),
                    ChassisNumber = ExtractChassisNumber(rawText),
                    Confidence = confidence
                };
            });
        }

        private static string? ExtractRegistrationNumber(
            string rawText)
        {
            var match = RegNumberPattern.Match(rawText.ToUpperInvariant());

            if (!match.Success)
            {
                return null;
            }

            return Regex.Replace(match.Value, @"[\s-]", string.Empty);
        }

        private static string? ExtractOwnerName(
            string rawText)
        {
            var lines = rawText.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("Owner", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("Name", StringComparison.OrdinalIgnoreCase))
                {
                    var colonIndex = trimmed.IndexOf(':');

                    if (colonIndex >= 0 && colonIndex < trimmed.Length - 1)
                    {
                        var value = trimmed[(colonIndex + 1)..].Trim();

                        return string.IsNullOrWhiteSpace(value) ? null : value;
                    }
                }
            }

            return null;
        }

        private static string? ExtractChassisNumber(
            string rawText)
        {
            foreach (Match match in ChassisPattern.Matches(rawText.ToUpperInvariant()))
            {
                // A chassis number is alphanumeric with at least one
                // digit and one letter - filters out pure-digit runs
                // (dates, phone numbers) the pattern would otherwise
                // also match.
                if (match.Value.Any(char.IsDigit) && match.Value.Any(char.IsLetter))
                {
                    return match.Value;
                }
            }

            return null;
        }
    }
}
