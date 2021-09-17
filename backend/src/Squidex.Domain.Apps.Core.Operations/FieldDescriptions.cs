// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core
{
    public static class FieldDescriptions
    {
        public static string AppId =>
            "The ID of the current app.";

        public static string AppName =>
            "The name of the current app.";

        public static string Asset =>
            "The asset.";

        public static string AssetFileHash =>
            "The hash of the file. Can be null for old files.";

        public static string AssetFileName =>
            "The file name of the asset.";

        public static string AssetFileSize =>
            "The size of the file in bytes.";

        public static string AssetFileType =>
            "The file type (file extension) of the asset.";

        public static string AssetFileVersion =>
            "The version of the file.";

        public static string AssetIsImage =>
            "Determines if the uploaded file is an image.";

        public static string AssetIsProtected =>
            "True, when the asset is not public.";

        public static string AssetMetadata =>
            "The asset metadata.";

        public static string AssetMetadataText =>
            "The type of the image.";

        public static string AssetMetadataValue =>
            "The asset metadata with name 'name'.";

        public static string AssetMimeType =>
            "The mime type.";

        public static string AssetParentId =>
            "The id of the parent folder. Empty for files without parent.";

        public static string AssetParentPath =>
            "The full path in the folder hierarchy as array of folder infos.";

        public static string AssetPixelHeight =>
            "The height of the image in pixels if the asset is an image.";

        public static string AssetPixelWidth =>
            "The width of the image in pixels if the asset is an image.";

        public static string AssetsItems =>
            "The assets.";

        public static string AssetSlug =>
            "The file name as slug.";

        public static string AssetSourceUrl =>
            "The source URL of the asset.";

        public static string AssetsTotal =>
            "The total count of assets.";

        public static string AssetTags =>
            "The asset tags.";

        public static string AssetThumbnailUrl =>
            "The thumbnail URL to the asset.";

        public static string AssetType =>
            "The type of the image.";

        public static string AssetUrl =>
            "The URL to the asset.";

        public static string Command =>
            "The executed command.";

        public static string ContentData =>
            "The data of the content.";

        public static string ContentDataOld =>
            "The previous data of the content.";

        public static string ContentFlatData =>
            "The flat data of the content.";

        public static string ContentNewStatus =>
            "The new status of the content.";

        public static string ContentNewStatusColor =>
            "The new status color of the content.";

        public static string ContentRequestData =>
            "The data for the content.";

        public static string ContentRequestDueTime =>
            "The timestamp when the status should be changed.";

        public static string ContentRequestOptionalId =>
            "The optional custom content ID.";

        public static string ContentRequestOptionalStatus =>
            "The initial status.";

        public static string ContentRequestPublish =>
            "Set to true to autopublish content on create.";

        public static string ContentRequestStatus =>
            "The status for the content.";

        public static string ContentSchema =>
            "The name of the schema.";

        public static string ContentSchemaId =>
            "The ID of the schema.";

        public static string ContentSchemaName =>
            "The display name of the schema.";

        public static string ContentsItems =>
            $"The contents.";

        public static string ContentStatus =>
            "The status of the content.";

        public static string ContentStatusColor =>
            "The status color of the content.";

        public static string ContentStatusOld =>
            "The previous status of the content.";

        public static string ContentsTotal =>
            $"The total count of  contents.";

        public static string ContentUrl =>
            "The URL to the content.";

        public static string Context =>
            "The context object holding all values.";

        public static string EntityCreated =>
            "The timestamp when the object was created.";

        public static string EntityCreatedBy =>
            "The user who created the object.";

        public static string EntityExpectedVersion =>
            "The expected version.";

        public static string EntityId =>
            "The ID of the object.";

        public static string EntityIsDeleted =>
            "True when deleted.";

        public static string EntityLastModified =>
            "The timestamp when the object was updated the last time.";

        public static string EntityLastModifiedBy =>
            "The user who updated the object the last time.";

        public static string EntityRequestDeletePermanent =>
            "True when the entity should be deleted permanently.";

        public static string EntityVersion =>
            "The version of the object (usually GUID).";

        public static string JsonPath =>
            "The path to the json value.";

        public static string Operation =>
            "The current operation.";

        public static string QueryFilter =>
            "Optional OData filter.";

        public static string QueryIds =>
            "Comma separated list of object IDs. Overrides all other query parameters.";

        public static string QueryOrderBy =>
            "Optional OData order definition.";

        public static string QueryQ =>
            "JSON query as well formatted json string. Overrides all other query parameters, except 'ids'.";

        public static string QuerySearch =>
            "Optional OData full text search.";

        public static string QuerySkip =>
            "Optional number of contents to skip.";

        public static string QueryTop =>
            "Optional number of contents to take.";

        public static string QueryVersion =>
            "The optional version of the content to retrieve an older instance (not cached).";

        public static string User =>
            "Information about the current user.";

        public static string UserClaims =>
            "The additional properties of the user.";

        public static string UserDisplayName =>
            "The display name of the user.";

        public static string UserEmail =>
            "The email address of the current user.";

        public static string UserId =>
            "The ID of the user.";

        public static string UserIsClient =>
            "True when the current user is a client, which is typically the case when the request is made from the API.";

        public static string UserIsUser =>
            "True when the current user is a user, which is typically the case when the request is made in the UI.";

        public static string UsersClaimsValue =>
            "The list of additional properties that have the name 'name'.";
    }
}
