using System.IO;
using System.IO.Compression;
using Raven.Server.Documents;
using Raven.Server.ServerWide.Context;
using Sparrow.Json;

namespace Raven.Server.Smuggler
{
    public class DatabaseDataExporter
    {
        private readonly DocumentDatabase _database;
        public long? StartDocsEtag;

        public int? Limit;

        public DatabaseDataExporter(DocumentDatabase database)
        {
            _database = database;
        }

        public ExportResult Export(DocumentsOperationContext context, string destinationFilePath)
        {
            using (var stream = File.Create(destinationFilePath))
            {
                return Export(context, stream);
            }
        }

        public ExportResult Export(DocumentsOperationContext context, Stream destinationStream)
        {
            long lastDocsEtag = 0;

            using (var gZipStream = new GZipStream(destinationStream, CompressionMode.Compress, leaveOpen: true))
            using (var writer = new BlittableJsonTextWriter(context, gZipStream))
            {
                writer.WriteStartObject();

                writer.WritePropertyName(context.GetLazyString("Docs"));
                var documents = Limit.HasValue
                      ? _database.DocumentsStorage.GetDocumentsAfter(context, StartDocsEtag ?? 0, 0, Limit.Value)
                      : _database.DocumentsStorage.GetDocumentsAfter(context, StartDocsEtag ?? 0);
                writer.WriteStartArray();
                bool first = true;
                foreach (var document in documents)
                {
                    if (document == null)
                        continue;

                    using (document.Data)
                    {
                        if (first == false)
                            writer.WriteComma();
                        first = false;

                        document.EnsureMetadata();
                        context.Write(writer, document.Data);
                        lastDocsEtag = document.Etag;
                    }
                }
                writer.WriteEndArray();

                writer.WriteComma();
                writer.WritePropertyName(context.GetLazyString("Attachments"));
                writer.WriteStartArray();
                writer.WriteEndArray();

                writer.WriteComma();
                writer.WritePropertyName(context.GetLazyString("Indexes"));
                writer.WriteStartArray();
                writer.WriteEndArray();

                writer.WriteComma();
                writer.WritePropertyName(context.GetLazyString("Identities"));
                writer.WriteStartArray();
                var identities = _database.DocumentsStorage.GetIdentities(context);
                first = true;
                foreach (var identity in identities)
                {
                    if (first == false)
                        writer.WriteComma();
                    first = false;

                    writer.WriteStartObject();
                    writer.WritePropertyName(context.GetLazyString("Key"));
                    writer.WriteString(context.GetLazyString(identity.Key));
                    writer.WriteComma();
                    writer.WritePropertyName(context.GetLazyString("Value"));
                    writer.WriteString(context.GetLazyString(identity.Value.ToString()));
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            return new ExportResult
            {
                LastDocsEtag = lastDocsEtag,
            };
        }
    }
}