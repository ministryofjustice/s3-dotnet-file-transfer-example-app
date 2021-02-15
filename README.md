# Example .NET Core S3 file transfer

A .NET core console app to transfer files to S3. 
Built using JetBrains Rider IDE

## Configure

Update the appSettings.json file to add AWS and S3 bucket configuration.

    "AWS": {
        "Profile": "example-profile",
        "Region": "eu-west-2"
    },

    "S3TaskSettings": {
        "BucketName":"<BUCKET_NAME>",
        "RecordingsPrefix":"<KEY_PREFIX>/",
        "MetadataPrefix":"<KEY_PREFIX>/"
    }

## Running

Usage: dotnet S3FileTransferApp.dll --recordingFile <localRecordingFile> --metadataFile <localMetadataFile>
Example: dotnet S3FileTransferApp.dll --recordingFile test-recording.flac --metadataFile test-metadata.json 



