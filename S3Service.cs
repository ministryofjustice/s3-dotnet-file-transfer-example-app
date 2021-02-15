using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace S3FileTransferApp
{
    internal class S3Service : IHostedService
    {
        private ILogger<S3Service> Logger { get; }
        private readonly IAmazonS3 _s3Client;
        private readonly S3TaskSettings _s3TaskSettings;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _config;

        public S3Service(ILogger<S3Service> logger, IAmazonS3 s3Client, IOptions<S3TaskSettings> s3TaskSettings,
            IHostApplicationLifetime hostApplicationLifetime, IConfiguration config)
        {
            Logger = logger;
            _s3Client = s3Client;
            _s3TaskSettings = s3TaskSettings.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Log(LogLevel.Information, "Starting Uploads...");
            UploadFileToS3();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void UploadFileToS3()
        {
            try
            {
                Logger.Log(LogLevel.Information, $@"recordingFile: {_config["recordingFile"]}" );
                Logger.Log(LogLevel.Information, $@"metadataFile: {_config["metadataFile"]}" );
                if (_config["recordingFile"] == null || _config["metadataFile"] == null)
                {
                    throw new ArgumentException("missing command line arguments");
                }

                if (_s3TaskSettings.BucketName == null || _s3TaskSettings.RecordingsPrefix == null
                                                       || _s3TaskSettings.MetaDataPrefix == null)
                {
                    throw new ArgumentException("missing S3 bucket settings");
                }

                using var utility = new TransferUtility(_s3Client);
                
                var recordingsPutRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _s3TaskSettings.BucketName,
                    Key = _s3TaskSettings.RecordingsPrefix + _config["recordingFile"],
                    FilePath = _config["recordingFile"],
                    StorageClass = S3StorageClass.StandardInfrequentAccess
                };

                Logger.Log(LogLevel.Information, "Uploading Recording file");
                await utility.UploadAsync(recordingsPutRequest);

                var metadataPutRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _s3TaskSettings.BucketName,
                    Key = _s3TaskSettings.MetaDataPrefix + _config["metadataFile"],
                    FilePath = _config["metadataFile"],
                    StorageClass = S3StorageClass.StandardInfrequentAccess
                };
                Logger.Log(LogLevel.Information, "Uploading Metadata file");
                await utility.UploadAsync(metadataPutRequest);
                Logger.Log(LogLevel.Information, "Finished Uploads");
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                Logger.Log(LogLevel.Error, $@"S3 error occurred. Exception: {amazonS3Exception}.");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $@"Exception: {ex}.");
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }
    }
}