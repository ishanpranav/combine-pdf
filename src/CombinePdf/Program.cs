using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.CommandLine;
using System.Reflection.PortableExecutable;

namespace CombinePdf;

internal static class Program
{
    private static void Main(string[] args)
    {
        Option<FileInfo[]> inputFilesOption = new Option<FileInfo[]>(
            name: "--input",
            "-i")
        {

            Description = "One or more input file paths (must exist)",
            Required = true,
            AllowMultipleArgumentsPerToken = true
        };

        inputFilesOption.Validators.Add(result =>
        {
            FileInfo[] inputFiles = result.GetValueOrDefault<FileInfo[]>();

            if (inputFiles == null || inputFiles.Length == 0)
            {
                result.AddError("At least one input file must be specified.");

                return;
            }

            foreach (FileInfo inputFile in inputFiles)
            {
                if (!inputFile.Exists)
                {
                    result.AddError($"Input file does not exist: {inputFile.FullName}");

                    return;
                }
            }
        });

        Option<FileInfo> outputFileOption = new Option<FileInfo>(
            name: "--output",
            "-o")
        {
            Required = true,
            Description = "Output file path (must be specified once)"
        };

        RootCommand rootCommand = new RootCommand("Example CLI tool with input(s) and output")
        {
            inputFilesOption,
            outputFileOption
        };

        try
        {
            ParseResult parseResult = rootCommand.Parse(args);

            FileInfo[]? inputFiles = parseResult
                .GetResult(inputFilesOption)!
                .GetValueOrDefault<FileInfo[]>()!;

            Console.WriteLine("Input files:");

            foreach (FileInfo inputFile in inputFiles)
            {
                Console.WriteLine($"  {inputFile.FullName}");
            }

            FileInfo outputFile = parseResult
                .GetResult(outputFileOption)!
                .GetValueOrDefault<FileInfo>()!;

            Console.WriteLine($"Output file: {outputFile.FullName}");

            CombinePdfs(inputFiles, outputFile);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void CombinePdfs(IEnumerable<FileInfo> inputFiles, FileInfo outputFile)
    {
        using (PdfDocument document = new PdfDocument())
        {
            foreach (FileInfo inputFile in inputFiles)
            {
                PdfDocument inputPdfDocument = PdfReader.Open(inputFile.Name, PdfDocumentOpenMode.Import);

                document.Version = Math.Max(document.Version, inputPdfDocument.Version);

                foreach (PdfPage page in inputPdfDocument.Pages)
                {
                    document.AddPage(page);
                }
            }

            document.Save(outputFile.Name);
        }
    }
}
