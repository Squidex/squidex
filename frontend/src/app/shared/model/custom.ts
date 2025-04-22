/* eslint-disable sort-imports */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { hasAnyLink, DateTime, StringHelper, Types, ApiUrlConfig, ErrorDto } from '@app/framework';
import * as generated from './generated';
import { FieldPropertiesVisitor, META_FIELDS, tableField, tableFields } from './schemas';

export class AppDto extends generated.AppDto {
    get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.label, this.name));
    }

    get canCreateSchema() {
        return this.compute('canCreateSchema', () => hasAnyLink(this._links, 'schemas/create'));
    }

    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canLeave() {
        return this.compute('canLeave', () => hasAnyLink(this._links, 'leave'));
    }

    get canReadAssets() {
        return this.compute('canReadAssets', () => hasAnyLink(this._links, 'assets'));
    }

    get canReadAssetsScripts() {
        return this.compute('canReadAssetsScripts', () => hasAnyLink(this._links, 'assets/scripts'));
    }

    get canReadClients() {
        return this.compute('canReadClients', () => hasAnyLink(this._links, 'clients'));
    }

    get canReadContributors() {
        return this.compute('canReadContributors', () => hasAnyLink(this._links, 'contributors'));
    }

    get canReadJobs() {
        return this.compute('canReadJobs', () => hasAnyLink(this._links, 'jobs'));
    }

    get canReadLanguages() {
        return this.compute('canReadLanguages', () => hasAnyLink(this._links, 'languages'));
    }

    get canReadPatterns() {
        return this.compute('canReadPatterns', () => hasAnyLink(this._links, 'patterns'));
    }

    get canReadPlans() {
        return this.compute('canReadPlans', () => hasAnyLink(this._links, 'plans'));
    }

    get canReadRoles() {
        return this.compute('canReadRoles', () => hasAnyLink(this._links, 'roles'));
    }

    get canReadRules() {
        return this.compute('canReadRules', () => hasAnyLink(this._links, 'rules'));
    }

    get canReadSchemas() {
        return this.compute('canReadSchemas', () => hasAnyLink(this._links, 'schemas'));
    }

    get canReadWorkflows() {
        return this.compute('canReadWorkflows', () => hasAnyLink(this._links, 'workflows'));
    }

    get canUpdateGeneral() {
        return this.compute('canUpdateGeneral', () => hasAnyLink(this._links, 'update'));
    }

    get canUpdateImage() {
        return this.compute('canUpdateImage', () => hasAnyLink(this._links, 'image/upload'));
    }

    get canUpdateTeam() {
        return this.compute('canUpdateTeam', () => hasAnyLink(this._links, 'transfer'));
    }

    get canUploadAssets() {
        return this.compute('canUploadAssets', () => hasAnyLink(this._links, 'assets/create'));
    }

    get image() {
        return this.compute('image', () => this._links['image']?.href);
    }
}

export class AppLanguageDto extends generated.AppLanguageDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class AppLanguagesDto extends generated.AppLanguagesDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class AppSettingsDto extends generated.AppSettingsDto {
    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class AssetDto extends generated.AssetDto {
    public get isDuplicate() {
        return this.compute('isDuplicate', () => this._meta && this._meta['isDuplicate'] === 'true');
    }

    public get contentUrl() {
        return this.compute('contentUrl', () => this._links['content']?.href);
    }

    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    get canMove() {
        return this.compute('canMove', () => hasAnyLink(this._links, 'move'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }

    get canUpload() {
        return this.compute('canUpload', () => hasAnyLink(this._links, 'upload'));
    }

    get canPreview() {
        return this.compute('canPreview', () => {
            const SVG_PREVIEW_LIMIT = 10 * 1024;
            const MIME_TIFF = 'image/tiff';
            const MIME_SVG = 'image/svg+xml';

            const canPreview =
                (this.mimeType !== MIME_TIFF && this.type === 'Image') ||
                (this.mimeType === MIME_SVG && this.fileSize < SVG_PREVIEW_LIMIT);

            return canPreview;
        });
    }

    public get fileNameWithoutExtension() {
        return this.compute('fileNameWithoutExtension', () => {
            const index = this.fileName.lastIndexOf('.');

            if (index > 0) {
                return this.fileName.substring(0, index);
            } else {
                return this.fileName;
            }

        });
    }

    public fullUrl(apiUrl: ApiUrlConfig) {
        return apiUrl.buildUrl(this.contentUrl);
    }
}

export class AssetsDto extends generated.AssetsDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    get canRenameTag() {
        return this.compute('canRenameTag', () => hasAnyLink(this._links, 'tags/rename'));
    }
}

export class AssetFolderDto extends generated.AssetFolderDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    get canMove() {
        return this.compute('canMove', () => hasAnyLink(this._links, 'move'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class AssetFoldersDto extends generated.AssetsDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class AssetScriptsDto extends generated.AssetScriptsDto {
    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class AuthSchemeResponseDto extends generated.AuthSchemeResponseDto {
    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class ClientDto extends generated.ClientDto {
    get canRevoke() {
        return this.compute('canRevoke', () => hasAnyLink(this._links, 'delete'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class ClientsDto extends generated.ClientsDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class ContentDto extends generated.ContentDto {
    get canPublish() {
        return this.compute('canPublish', () => this.statusUpdates.find(x => x.status === 'Published'));
    }

    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    get canDraftCreate() {
        return this.compute('canDraftCreate', () => hasAnyLink(this._links, 'draft/create'));
    }

    get canDraftDelete() {
        return this.compute('canDraftDelete', () => hasAnyLink(this._links, 'draft/delete'));
    }

    get canCancelStatus() {
        return this.compute('canCancelStatus', () => hasAnyLink(this._links, 'cancel'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }

    get statusUpdates() {
        return this.compute('statusUpdates', () => {
            const updates: { status: string; color: string }[] = [];
            for (const [key, link] of Object.entries(this._links)) {
                if (key.startsWith('status/')) {
                    updates.push({ status: key.substring(7), color: link.metadata! });
                }
            }

            return updates;
        });
    }
}

export class ContentsDto extends generated.ContentsDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    get canCreateAndPublish() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create/publish'));
    }
}

export class ContributorDto extends generated.ContributorDto {
    public get token() {
        return `subject:${this.contributorId}`;
    }

    get canRevoke() {
        return this.compute('canRevoke ', () => hasAnyLink(this._links, 'update'));
    }

    get canUpdate() {
        return this.compute('canUpdate  ', () => hasAnyLink(this._links, 'update'));
    }
}

export class ContributorsDto extends generated.ContributorsDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    get isInvited() {
        return this.compute('isInvited', () => this._meta?.['isInvited'] === '1');
    }
}

export class EventConsumerDto extends generated.UserDto {
    get canReset() {
        return this.compute('reset', () => hasAnyLink(this._links, 'reset'));
    }

    get canStart() {
        return this.compute('canStart', () => hasAnyLink(this._links, 'start'));
    }

    get canStop() {
        return this.compute('canStop', () => hasAnyLink(this._links, 'stop'));
    }
}

export class FieldDto extends generated.FieldDto {
    public get rawProperties(): any {
        return this.properties;
    }

    public get isInlineEditable(): boolean {
        return this.compute('isInlineEditable', () => !this.isDisabled && this.rawProperties.inlineEditable === true);
    }

    public get isInvariant(): boolean {
        return this.compute('isInvariant', () => this.partitioning === 'invariant');
    }

    public get isLocalizable(): boolean {
        return this.compute('isLocalizable', () => this.partitioning === 'language');
    }

    public get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.properties.label, this.name));
    }

    public get displayPlaceholder() {
        return this.compute('displayPlaceholder', () => this.properties.placeholder || '');
    }

    get canAddField() {
        return this.compute('canAddField', () => hasAnyLink(this._links, 'fields/add'));
    }

    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canDisable() {
        return this.compute('canDisable', () => hasAnyLink(this._links, 'disable'));
    }

    get canEnable() {
        return this.compute('canEnable', () => hasAnyLink(this._links, 'enable'));
    }

    get canOrderFields() {
        return this.compute('canOrderFields', () => hasAnyLink(this._links, 'fields/order'));
    }

    get canHide() {
        return this.compute('canHide', () => hasAnyLink(this._links, 'hide'));
    }

    get canLock() {
        return this.compute('canLock', () => hasAnyLink(this._links, 'lock'));
    }

    get canShow() {
        return this.compute('canShow', () => hasAnyLink(this._links, 'show'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class NestedFieldDto extends generated.NestedFieldDto {
    public get rawProperties(): any {
        return this.properties;
    }

    public get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.properties.label, this.name));
    }

    public get displayPlaceholder() {
        return this.compute('displayPlaceholder', () => this.properties.placeholder || '');
    }

    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canDisable() {
        return this.compute('canDisable', () => hasAnyLink(this._links, 'disable'));
    }

    get canEnable() {
        return this.compute('canEnable', () => hasAnyLink(this._links, 'enable'));
    }
    get canHide() {
        return this.compute('canHide', () => hasAnyLink(this._links, 'hide'));
    }

    get canLock() {
        return this.compute('canLock', () => hasAnyLink(this._links, 'lock'));
    }

    get canShow() {
        return this.compute('canShow', () => hasAnyLink(this._links, 'show'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class IndexDto extends generated.IndexDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }
}

export class IndexesDto extends generated.IndexesDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class JobDto extends generated.JobDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canDownloadUrl() {
        return this.compute('canDownloadUrl', () => hasAnyLink(this._links, 'download'));
    }

    get downloadUrl() {
        return this.compute('downloadUrl', () => this._links['download']?.href);
    }

    get isfailed() {
        return this.status === 'Failed';
    }
}

export class JobsDto extends generated.JobsDto {
    get canCreateBackup() {
        return this.compute('canCreateBackup', () => hasAnyLink(this._links, 'create/backups'));
    }
}

export class RoleDto extends generated.RoleDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class RolesDto extends generated.RolesDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class SchemaDto extends generated.SchemaDto {
    get canAddField() {
        return this.compute('canAddField', () => hasAnyLink(this._links, 'fields/add'));
    }

    get canContentsCreate() {
        return this.compute('canContentsCreate', () => hasAnyLink(this._links, 'contents/create'));
    }

    get canContentsCreateAndPublish() {
        return this.compute('canContentsCreateAndPublish', () => hasAnyLink(this._links, 'contents/create/publish'));
    }

    get canContentsRead() {
        return this.compute('canContentsCreateAndPublish', () => hasAnyLink(this._links, 'contents'));
    }

    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canOrderFields() {
        return this.compute('canOrderFields', () => hasAnyLink(this._links, 'fields/order'));
    }

    get canPublish() {
        return this.compute('canPublish', () => hasAnyLink(this._links, 'publish'));
    }

    get canReadContents() {
        return this.compute('canReadContents', () => hasAnyLink(this._links, 'contents'));
    }

    get canSynchronize() {
        return this.compute('canReadContents', () => hasAnyLink(this._links, 'update/sync'));
    }

    get canUnpublish() {
        return this.compute('canReadContents', () => hasAnyLink(this._links, 'unpublish'));
    }

    get canUpdate() {
        return this.compute('canReadContents', () => hasAnyLink(this._links, 'update'));
    }

    get canUpdateCategory() {
        return this.compute('canUpdateCategory', () => hasAnyLink(this._links, 'update/category'));
    }

    get canUpdateRules() {
        return this.compute('canUpdateCategory', () => hasAnyLink(this._links, 'update/rules'));
    }

    get canUpdateScripts() {
        return this.compute('canUpdateScripts', () => hasAnyLink(this._links, 'update/scripts'));
    }

    get canUpdateUIFields() {
        return this.compute('canUpdateUIFields', () => hasAnyLink(this._links, 'fields/ui'));
    }

    get canUpdateUrls() {
        return this.compute('canUpdateUrls', () => hasAnyLink(this._links, 'update/urls'));
    }

    get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.properties.label, this.name));
    }

    get contentFields() {
        return this.compute('displayName', () => this.fields.filter(x => x.properties.isContentField).map(tableField));
    }

    get defaultListFields() {
        return this.compute('defaultListFields', () => {
            const listFields = tableFields(this.fieldsInLists, this.contentFields);

            if (listFields.length === 0) {
                listFields.push(META_FIELDS.lastModifiedByAvatar);

                if (this.fields.length > 0) {
                    listFields.push(tableField(this.fields[0]));
                } else {
                    listFields.push(META_FIELDS.empty);
                }

                listFields.push(META_FIELDS.statusColor);
                listFields.push(META_FIELDS.lastModified);
            }

            return listFields;
        });
    }

    get defaultReferenceFields() {
        return this.compute('defaultReferenceFields', () => {
            const referenceFields = tableFields(this.fieldsInReferences, this.contentFields);

            if (referenceFields.length === 0) {
                if (this.fields.length > 0) {
                    referenceFields.push(tableField(this.fields[0]));
                } else {
                    referenceFields.push(META_FIELDS.empty);
                }
            }

            return referenceFields;
        });
    }

    public export(): any {
        const fieldKeys = [
            'fieldId',
            'parentId',
            'parentFieldId',
            '_links',
        ];

        const cleanup = (source: any, ...exclude: string[]): any => {
            const clone = {} as Record<string, any>;

            for (const [key, value] of Object.entries(source)) {
                if (!exclude.includes(key) && key.indexOf('can') !== 0 && !Types.isUndefined(value) && !Types.isNull(value)) {
                    clone[key] = value;
                }
            }

            return clone;
        };

        const result: any = {
            previewUrls: this.previewUrls,
            properties: cleanup(this.properties),
            category: this.category,
            scripts: this.scripts,
            isPublished: this.isPublished,
            fieldRules: this.fieldRules,
            fieldsInLists: this.fieldsInLists,
            fieldsInReferences: this.fieldsInReferences,
            fields: this.fields.map(field => {
                const copy = cleanup(field, ...fieldKeys);

                copy.properties = cleanup(field.properties);

                if (Types.isArray(copy.nested)) {
                    if (copy.nested.length === 0) {
                        delete copy['nested'];
                    } else if (field.nested) {
                        copy.nested = field.nested.map(nestedField => {
                            const nestedCopy = cleanup(nestedField, ...fieldKeys);

                            nestedCopy.properties = cleanup(nestedField.properties);

                            return nestedCopy;
                        });
                    }
                }

                return copy;
            }),
            type: this.type,
        };

        return result;
    }
}

export class DynamicRuleDto extends generated.DynamicRuleDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canDisable() {
        return this.compute('canDisable', () => hasAnyLink(this._links, 'disable'));
    }

    get canEnable() {
        return this.compute('canEnable', () => hasAnyLink(this._links, 'enable'));
    }

    get canReadLogs() {
        return this.compute('canReadLogs', () => hasAnyLink(this._links, 'logs'));
    }

    get canRun() {
        return this.compute('canRun', () => hasAnyLink(this._links, 'run'));
    }

    get canRunFromSnapshots() {
        return this.compute('canRunFromSnapshots', () => hasAnyLink(this._links, 'run/snapshots'));
    }

    get canTrigger() {
        return this.compute('canTrigger', () => hasAnyLink(this._links, 'trigger'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class DynamicRulesDto extends generated.DynamicRulesDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    get canReadEvents() {
        return this.compute('canReadEvents', () => hasAnyLink(this._links, 'events'));
    }

    get canCancelRun() {
        return this.compute('canCancelRun', () => hasAnyLink(this._links, 'run/cancel'));
    }
}

export class RuleEventDto extends generated.RuleEventDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'cancel'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class RuleEventsDto extends generated.RuleEventsDto {
    get canCancelAll() {
        return this.compute('canCancelAll ', () => hasAnyLink(this._links, 'cancel'));
    }
}

export class SearchResultDto extends generated.SearchResultDto {
    get url() {
        return this.compute('url', () => this._links['url'].href);
    }
}

export class TeamDto extends generated.TeamDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canReadAuth() {
        return this.compute('canReadAuth', () => hasAnyLink(this._links, 'auth'));
    }

    get canReadContributors() {
        return this.compute('canReadContributors', () => hasAnyLink(this._links, 'contributors'));
    }

    get canReadPlans() {
        return this.compute('canReadPlans', () => hasAnyLink(this._links, 'plans'));
    }

    get canUpdateGeneral() {
        return this.compute('canUpdateGeneral', () => hasAnyLink(this._links, 'update'));
    }
}

export class SchemasDto extends generated.SchemasDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class ServerErrorDto extends generated.ServerErrorDto {
    toError(): ErrorDto {
        return new ErrorDto(
            this.statusCode,
            this.message,
            this.errorCode,
            this.details);
    }
}

export class UserDto extends generated.UserDto {
    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canLock() {
        return this.compute('canLock', () => hasAnyLink(this._links, 'lock'));
    }

    get canUnlock() {
        return this.compute('canUnlock', () => hasAnyLink(this._links, 'unlock'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class UsersDto extends generated.UsersDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class WorkflowDto extends generated.WorkflowDto {
    get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.name, 'i18n:workflows.notNamed'));
    }

    get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class WorkflowsDto extends generated.WorkflowsDto {
    get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

//
// FIELD TYPES
//
export class FieldPropertiesDto extends generated.FieldPropertiesDto {
    public get isComplexUI() {
        return true;
    }

    public get isSortable() {
        return true;
    }

    public get isContentField() {
        return true;
    }

    public accept<T>(_visitor: FieldPropertiesVisitor<T>): T {
        throw new Error('NOT IMPLEMENTED');
    }
}

export class ArrayFieldPropertiesDto extends generated.ArrayFieldPropertiesDto {
    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitArray(this);
    }
}

export class AssetsFieldPropertiesDto extends generated.AssetsFieldPropertiesDto {
    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitAssets(this);
    }
}

export class BooleanFieldPropertiesDto extends generated.BooleanFieldPropertiesDto {
    public get isComplexUI() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitBoolean(this);
    }
}

export class ComponentFieldPropertiesDto extends generated.ComponentFieldPropertiesDto {
    public get isComplexUI() {
        return true;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitComponent(this);
    }
}

export class ComponentsFieldPropertiesDto extends generated.ComponentsFieldPropertiesDto {
    public get isComplexUI() {
        return true;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitComponents(this);
    }
}

export class DateTimeFieldPropertiesDto extends generated.DateTimeFieldPropertiesDto {
    public get isComplexUI() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitDateTime(this);
    }
}

export class GeolocationFieldPropertiesDto extends generated.GeolocationFieldPropertiesDto {
    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitGeolocation(this);
    }
}

export class JsonFieldPropertiesDto extends generated.JsonFieldPropertiesDto {
    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitJson(this);
    }
}

export class NumberFieldPropertiesDto extends generated.NumberFieldPropertiesDto  {
    public get isComplexUI() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitNumber(this);
    }
}

export class ReferencesFieldPropertiesDto extends generated.ReferencesFieldPropertiesDto {
    public get singleId() {
        return this.schemaIds?.[0] || null;
    }

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitReferences(this);
    }
}

export class RichTextFieldPropertiesDto extends generated.RichTextFieldPropertiesDto {
    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitRichText(this);
    }
}

export class StringFieldPropertiesDto extends generated.StringFieldPropertiesDto {
    public get isComplexUI() {
        return this.editor !== 'Input' && this.editor !== 'Color' && this.editor !== 'Radio' && this.editor !== 'Slug' && this.editor !== 'TextArea';
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitString(this);
    }
}

export class TagsFieldPropertiesDto extends generated.TagsFieldPropertiesDto {
    public get isComplexUI() {
        return false;
    }

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitTags(this);
    }
}

export class UIFieldPropertiesDto extends generated.UIFieldPropertiesDto {
    public get isComplexUI() {
        return false;
    }

    public get isSortable() {
        return false;
    }

    public get isContentField() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitUI(this);
    }
}
