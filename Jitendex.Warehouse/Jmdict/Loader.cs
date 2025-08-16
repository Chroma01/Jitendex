using System.Xml;
using Jitendex.Warehouse.Jmdict.Models;

namespace Jitendex.Warehouse.Jmdict;

public static class Loader
{
    public async static IAsyncEnumerable<Entry> Entries(string path)
    {
        await using var stream = File.OpenRead(path);

        var readerSettings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = long.MaxValue,
            MaxCharactersInDocument = long.MaxValue,
        };

        using var reader = XmlReader.Create(stream, readerSettings);
        var docMeta = new DocumentMetadata
        {
            Name = string.Empty,
            EntityValueToName = [],
        };

        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    docMeta = await DocumentMetadata.FromXmlAsync(reader);
                    break;
                case XmlNodeType.Element:
                    if (reader.Name == Entry.XmlTagName)
                    {
                        var entry = await Entry.FromXmlAsync(reader, docMeta);
                        yield return entry;
                    }
                    break;
            }
        }
    }
}
