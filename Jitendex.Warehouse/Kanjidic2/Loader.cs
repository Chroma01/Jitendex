using System.Xml;
using Jitendex.Warehouse.Kanjidic2.Models;

namespace Jitendex.Warehouse.Kanjidic2;

public class Loader
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

        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name == Entry.XmlTagName)
                    {
                        var entry = await Entry.FromXmlAsync(reader);
                        yield return entry;
                    }
                    break;
                default:
                    break;
            }
        }
    }

}
