/* eslint-disable sort-imports */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { hasAnyLink, StringHelper, Types, ApiUrlConfig, ErrorDto } from '@app/framework';
import * as generated from './generated';
import { FieldPropertiesVisitor, META_FIELDS, tableField, tableFields } from './schemas';

export class AppDto extends generated.AppDto {
    public get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.label, this.name));
    }

    public get canCreateSchema() {
        return this.compute('canCreateSchema', () => hasAnyLink(this._links, 'schemas/create'));
    }

    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canLeave() {
        return this.compute('canLeave', () => hasAnyLink(this._links, 'leave'));
    }

    public get canReadAssets() {
        return this.compute('canReadAssets', () => hasAnyLink(this._links, 'assets'));
    }

    public get canReadAssetsScripts() {
        return this.compute('canReadAssetsScripts', () => hasAnyLink(this._links, 'assets/scripts'));
    }

    public get canReadClients() {
        return this.compute('canReadClients', () => hasAnyLink(this._links, 'clients'));
    }

    public get canReadContributors() {
        return this.compute('canReadContributors', () => hasAnyLink(this._links, 'contributors'));
    }

    public get canReadJobs() {
        return this.compute('canReadJobs', () => hasAnyLink(this._links, 'jobs'));
    }

    public get canReadLanguages() {
        return this.compute('canReadLanguages', () => hasAnyLink(this._links, 'languages'));
    }

    public get canReadPatterns() {
        return this.compute('canReadPatterns', () => hasAnyLink(this._links, 'patterns'));
    }

    public get canReadPlans() {
        return this.compute('canReadPlans', () => hasAnyLink(this._links, 'plans'));
    }

    public get canReadRoles() {
        return this.compute('canReadRoles', () => hasAnyLink(this._links, 'roles'));
    }

    public get canReadRules() {
        return this.compute('canReadRules', () => hasAnyLink(this._links, 'rules'));
    }

    public get canReadSchemas() {
        return this.compute('canReadSchemas', () => hasAnyLink(this._links, 'schemas'));
    }

    public get canReadWorkflows() {
        return this.compute('canReadWorkflows', () => hasAnyLink(this._links, 'workflows'));
    }

    public get canUpdateGeneral() {
        return this.compute('canUpdateGeneral', () => hasAnyLink(this._links, 'update'));
    }

    public get canUpdateImage() {
        return this.compute('canUpdateImage', () => hasAnyLink(this._links, 'image/upload'));
    }

    public get canUpdateTeam() {
        return this.compute('canUpdateTeam', () => hasAnyLink(this._links, 'transfer'));
    }

    public get canUploadAssets() {
        return this.compute('canUploadAssets', () => hasAnyLink(this._links, 'assets/create'));
    }

    public get image() {
        return this.compute('image', () => this._links['image']?.href);
    }
}

export class AppLanguageDto extends generated.AppLanguageDto {
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class AppLanguagesDto extends generated.AppLanguagesDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class AppSettingsDto extends generated.AppSettingsDto {
    public get canUpdate() {
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

    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    public get canMove() {
        return this.compute('canMove', () => hasAnyLink(this._links, 'move'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }

    public get canUpload() {
        return this.compute('canUpload', () => hasAnyLink(this._links, 'upload'));
    }

    public get canPreview() {
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
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    public get canRenameTag() {
        return this.compute('canRenameTag', () => hasAnyLink(this._links, 'tags/rename'));
    }
}

export class AssetFolderDto extends generated.AssetFolderDto {
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    public get canMove() {
        return this.compute('canMove', () => hasAnyLink(this._links, 'move'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class AssetFoldersDto extends generated.AssetsDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class AssetScriptsDto extends generated.AssetScriptsDto {
    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class AuthSchemeResponseDto extends generated.AuthSchemeResponseDto {
    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class ClientDto extends generated.ClientDto {
    public get canRevoke() {
        return this.compute('canRevoke', () => hasAnyLink(this._links, 'delete'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class ClientsDto extends generated.ClientsDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class ContentDto extends generated.ContentDto {
    public get canPublish() {
        return this.compute('canPublish', () => this.statusUpdates.find(x => x.status === 'Published'));
    }

    public get canClone() {
        return this.compute('canClone', () => hasAnyLink(this._links, 'clone'));
    }

    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    public get canDraftCreate() {
        return this.compute('canDraftCreate', () => hasAnyLink(this._links, 'draft/create'));
    }

    public get canDraftDelete() {
        return this.compute('canDraftDelete', () => hasAnyLink(this._links, 'draft/delete'));
    }

    public get canCancelStatus() {
        return this.compute('canCancelStatus', () => hasAnyLink(this._links, 'cancel'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }

    public get statusUpdates() {
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
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    public get canCreateAndPublish() {
        return this.compute('canCreateAndPublish', () => hasAnyLink(this._links, 'create/publish'));
    }
}

export class ContributorDto extends generated.ContributorDto {
    public get token() {
        return `subject:${this.contributorId}`;
    }

    public get canRevoke() {
        return this.compute('canRevoke', () => hasAnyLink(this._links, 'update'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class ContributorsDto extends generated.ContributorsDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    public get isInvited() {
        return this.compute('isInvited', () => this._meta?.['isInvited'] === '1');
    }
}

export class EventConsumerDto extends generated.UserDto {
    public get canReset() {
        return this.compute('canReset', () => hasAnyLink(this._links, 'reset'));
    }

    public get canStart() {
        return this.compute('canStart', () => hasAnyLink(this._links, 'start'));
    }

    public get canStop() {
        return this.compute('canStop', () => hasAnyLink(this._links, 'stop'));
    }
}

export class FieldDto extends generated.FieldDto {
    public get rawProperties(): any {
        return this.properties;
    }

    public get isInlineEditable() {
        return this.compute('isInlineEditable', () => !this.isDisabled && this.rawProperties.inlineEditable === true);
    }

    public get isInvariant() {
        return this.compute('isInvariant', () => this.partitioning === 'invariant');
    }

    public get isLocalizable() {
        return this.compute('isLocalizable', () => this.partitioning === 'language');
    }

    public get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.properties.label, this.name));
    }

    public get displayPlaceholder() {
        return this.compute('displayPlaceholder', () => this.properties.placeholder || '');
    }

    public get canAddField() {
        return this.compute('canAddField', () => hasAnyLink(this._links, 'fields/add'));
    }

    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canDisable() {
        return this.compute('canDisable', () => hasAnyLink(this._links, 'disable'));
    }

    public get canEnable() {
        return this.compute('canEnable', () => hasAnyLink(this._links, 'enable'));
    }

    public get canOrderFields() {
        return this.compute('canOrderFields', () => hasAnyLink(this._links, 'fields/order'));
    }

    public get canHide() {
        return this.compute('canHide', () => hasAnyLink(this._links, 'hide'));
    }

    public get canLock() {
        return this.compute('canLock', () => hasAnyLink(this._links, 'lock'));
    }

    public get canShow() {
        return this.compute('canShow', () => hasAnyLink(this._links, 'show'));
    }

    public get canUpdate() {
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

    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canDisable() {
        return this.compute('canDisable', () => hasAnyLink(this._links, 'disable'));
    }

    public get canEnable() {
        return this.compute('canEnable', () => hasAnyLink(this._links, 'enable'));
    }
    public get canHide() {
        return this.compute('canHide', () => hasAnyLink(this._links, 'hide'));
    }

    public get canLock() {
        return this.compute('canLock', () => hasAnyLink(this._links, 'lock'));
    }

    public get canShow() {
        return this.compute('canShow', () => hasAnyLink(this._links, 'show'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class IndexDto extends generated.IndexDto {
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }
}

export class IndexesDto extends generated.IndexesDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class JobDto extends generated.JobDto {
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canDownloadUrl() {
        return this.compute('canDownloadUrl', () => hasAnyLink(this._links, 'download'));
    }

    public get downloadUrl() {
        return this.compute('downloadUrl', () => this._links['download']?.href);
    }

    public get isfailed() {
        return this.status === 'Failed';
    }
}

export class JobsDto extends generated.JobsDto {
    public get canCreateBackup() {
        return this.compute('canCreateBackup', () => hasAnyLink(this._links, 'create/backups'));
    }
}

export class RoleDto extends generated.RoleDto {
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'update'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class RolesDto extends generated.RolesDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class SchemaDto extends generated.SchemaDto {
    public get canAddField() {
        return this.compute('canAddField', () => hasAnyLink(this._links, 'fields/add'));
    }

    public get canContentsCreate() {
        return this.compute('canContentsCreate', () => hasAnyLink(this._links, 'contents/create'));
    }

    public get canContentsCreateAndPublish() {
        return this.compute('canContentsCreateAndPublish', () => hasAnyLink(this._links, 'contents/create/publish'));
    }

    public get canContentsRead() {
        return this.compute('canContentsRead', () => hasAnyLink(this._links, 'contents'));
    }

    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canOrderFields() {
        return this.compute('canOrderFields', () => hasAnyLink(this._links, 'fields/order'));
    }

    public get canPublish() {
        return this.compute('canPublish', () => hasAnyLink(this._links, 'publish'));
    }

    public get canReadContents() {
        return this.compute('canReadContents', () => hasAnyLink(this._links, 'contents'));
    }

    public get canSynchronize() {
        return this.compute('canSynchronize', () => hasAnyLink(this._links, 'update/sync'));
    }

    public get canUnpublish() {
        return this.compute('canUnpublish', () => hasAnyLink(this._links, 'unpublish'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }

    public get canUpdateCategory() {
        return this.compute('canUpdateCategory', () => hasAnyLink(this._links, 'update/category'));
    }

    public get canUpdateRules() {
        return this.compute('canUpdateRules', () => hasAnyLink(this._links, 'update/rules'));
    }

    public get canUpdateScripts() {
        return this.compute('canUpdateScripts', () => hasAnyLink(this._links, 'update/scripts'));
    }

    public get canUpdateUIFields() {
        return this.compute('canUpdateUIFields', () => hasAnyLink(this._links, 'fields/ui'));
    }

    public get canUpdateUrls() {
        return this.compute('canUpdateUrls', () => hasAnyLink(this._links, 'update/urls'));
    }

    public get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.properties.label, this.name));
    }

    public get contentFields() {
        return this.compute('contentFields', () => this.fields.filter(x => x.properties.isContentField).map(tableField));
    }

    public get defaultListFields() {
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

    public get defaultReferenceFields() {
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
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canDisable() {
        return this.compute('canDisable', () => hasAnyLink(this._links, 'disable'));
    }

    public get canEnable() {
        return this.compute('canEnable', () => hasAnyLink(this._links, 'enable'));
    }

    public get canReadLogs() {
        return this.compute('canReadLogs', () => hasAnyLink(this._links, 'logs'));
    }

    public get canRun() {
        return this.compute('canRun', () => hasAnyLink(this._links, 'run'));
    }

    public get canRunFromSnapshots() {
        return this.compute('canRunFromSnapshots', () => hasAnyLink(this._links, 'run/snapshots'));
    }

    public get canTrigger() {
        return this.compute('canTrigger', () => hasAnyLink(this._links, 'trigger'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class DynamicRulesDto extends generated.DynamicRulesDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }

    public get canReadEvents() {
        return this.compute('canReadEvents', () => hasAnyLink(this._links, 'events'));
    }

    public get canCancelRun() {
        return this.compute('canCancelRun', () => hasAnyLink(this._links, 'run/cancel'));
    }
}

export class RuleEventDto extends generated.RuleEventDto {
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'cancel'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class RuleEventsDto extends generated.RuleEventsDto {
    public get canCancelAll() {
        return this.compute('canCancelAll', () => hasAnyLink(this._links, 'cancel'));
    }
}

export class SearchResultDto extends generated.SearchResultDto {
    public get url() {
        return this.compute('url', () => this._links['url'].href);
    }
}

export class TeamDto extends generated.TeamDto {
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canReadAuth() {
        return this.compute('canReadAuth', () => hasAnyLink(this._links, 'auth'));
    }

    public get canReadContributors() {
        return this.compute('canReadContributors', () => hasAnyLink(this._links, 'contributors'));
    }

    public get canReadPlans() {
        return this.compute('canReadPlans', () => hasAnyLink(this._links, 'plans'));
    }

    public get canUpdateGeneral() {
        return this.compute('canUpdateGeneral', () => hasAnyLink(this._links, 'update'));
    }
}

export class SchemasDto extends generated.SchemasDto {
    public get canCreate() {
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
    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canLock() {
        return this.compute('canLock', () => hasAnyLink(this._links, 'lock'));
    }

    public get canUnlock() {
        return this.compute('canUnlock', () => hasAnyLink(this._links, 'unlock'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class UsersDto extends generated.UsersDto {
    public get canCreate() {
        return this.compute('canCreate', () => hasAnyLink(this._links, 'create'));
    }
}

export class WorkflowDto extends generated.WorkflowDto {
    public get displayName() {
        return this.compute('displayName', () => StringHelper.firstNonEmpty(this.name, 'i18n:workflows.notNamed'));
    }

    public get canDelete() {
        return this.compute('canDelete', () => hasAnyLink(this._links, 'delete'));
    }

    public get canUpdate() {
        return this.compute('canUpdate', () => hasAnyLink(this._links, 'update'));
    }
}

export class WorkflowsDto extends generated.WorkflowsDto {
    public get canCreate() {
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

export class UserInfoFieldPropertiesDto extends generated.UserInfoFieldPropertiesDto {
    public get isComplexUI() {
        return true;
    }

    public get isSortable() {
        return false;
    }

    public get isContentField() {
        return true;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitUserInfo(this);
    }
}
