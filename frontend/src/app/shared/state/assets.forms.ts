/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { applyEach, maxLength, required } from '@angular/forms/signals';
import { generateSlug, SignalForm, SubmitAction, Types } from '@app/framework';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, MoveAssetDto, RenameAssetFolderDto, RenameTagDto, UpdateAssetScriptsDto } from '../model';

export type AssetMetadataFormModel = { name: string; value: string };

export type AnnotateAssetFormModel = {
    isProtected: boolean;
    fileName: string;
    slug: string;
    tags: string[];
    metadata: AssetMetadataFormModel[];
};

export class AnnotateAssetForm extends SignalForm<AnnotateAssetFormModel, AnnotateAssetDto, AssetDto> {
    constructor() {
        super({ isProtected: false, fileName: '', slug: '', tags: [], metadata: [] }, path => {
            applyEach(path.metadata, item => {
                maxLength(item.name, 1000);
                required(item.name);
            });
            required(path.fileName);
            required(path.slug);
        });
    }

    public addMetadata() {
        this.model.update(x => ({ ...x, metadata: [...x.metadata, { name: '', value: '' }] }));
    }

    public removeMetadata(index: number) {
        this.model.update(x => ({ ...x, metadata: x.metadata.filter((_, i) => i !== index) }));
    }

    protected transformSubmit(value: AnnotateAssetFormModel) {
        const metadata: Record<string, any> = {};

        for (const item of value.metadata) {
            const raw = item.value;

            let parsed: any = raw;

            if (raw) {
                try {
                    parsed = JSON.parse(raw);
                } catch {
                    parsed = raw;
                }
            }

            if (parsed === '') {
                parsed = null;
            }

            metadata[item.name] = parsed;
        }

        return new AnnotateAssetDto({ ...value, metadata });
    }

    public submitChanges(asset: AssetDto, action: SubmitAction<AnnotateAssetDto | null, unknown>) {
        return this.submit(value => {
            return action(getChanges(asset, value));
        });
    }

    protected transformLoad(value: Partial<AssetDto>): AnnotateAssetFormModel {
        let fileName = value.fileName ?? '';

        const index = fileName.lastIndexOf('.');
        if (index > 0) {
            fileName = fileName.substring(0, index);
        }

        const metadata: AssetMetadataFormModel[] = [];

        if (Types.isObject(value.metadata)) {
            for (const [name, raw] of Object.entries(value.metadata)) {
                let converted = '';

                if (Types.isString(raw)) {
                    converted = raw;
                } else if (!Types.isUndefined(raw) && !Types.isNull(raw)) {
                    converted = JSON.stringify(raw);
                }

                metadata.push({ name, value: converted });
            }
        }

        return {
            isProtected: value.isProtected ?? false,
            fileName,
            slug: value.slug ?? '',
            tags: value.tags ?? [],
            metadata,
        };
    }

    public generateSlug(asset: AssetDto) {
        const fileName = this.model().fileName;
        if (!fileName) {
            return;
        }

        let slug = generateSlug(fileName, { allowed: ['.'] });

        if (asset.fileName) {
            const index = asset.fileName.lastIndexOf('.');

            if (index > 0) {
                slug += asset.fileName.substring(index);
            }
        }

        this.form.slug().value.set(slug);
    }
}

const ASSET_SCRIPT_NAMES = ['query', 'queryPre', 'annotate', 'create', 'delete', 'move', 'update'] as const;

export type AssetScriptName = typeof ASSET_SCRIPT_NAMES[number];

export type EditAssetScriptsFormModel = Record<AssetScriptName, string>;

export class EditAssetScriptsForm extends SignalForm<EditAssetScriptsFormModel, UpdateAssetScriptsDto, Partial<EditAssetScriptsFormModel>> {
    public readonly scriptNames = ASSET_SCRIPT_NAMES;

    constructor() {
        super({ query: '', queryPre: '', annotate: '', create: '', delete: '', move: '', update: '' });
    }

    protected transformSubmit(value: EditAssetScriptsFormModel) {
        return new UpdateAssetScriptsDto(value);
    }
}

export class RenameAssetFolderForm extends SignalForm<{ folderName: string }, RenameAssetFolderDto, AssetFolderDto> {
    constructor() {
        super({ folderName: '' }, path => {
            required(path.folderName);
        });
    }

    protected transformSubmit(value: { folderName: string }) {
        return new RenameAssetFolderDto(value);
    }
}

export class RenameAssetTagForm extends SignalForm<{ tagName: string }, RenameTagDto, RenameTagDto> {
    constructor() {
        super({ tagName: '' }, path => {
            required(path.tagName);
        });
    }

    protected transformSubmit(value: { tagName: string }) {
        return new RenameTagDto(value);
    }
}

export class MoveAssetForm extends SignalForm<{ parentId: string }, MoveAssetDto, AssetDto> {
    constructor() {
        super({ parentId: '' }, path => {
            required(path.parentId);
        });
    }

    protected transformSubmit(value: { parentId: string }) {
        return new MoveAssetDto(value);
    }
}

function getChanges(asset: AssetDto, value: AnnotateAssetDto) {
    const result: Record<string, any> = {};

    let fileName = value.fileName;

    const index = asset.fileName.lastIndexOf('.');
    if (index > 0) {
        fileName += asset.fileName.substring(index);
    }

    if (fileName !== asset.fileName) {
        result['fileName'] = fileName;
    }

    if (value.slug !== asset.slug) {
        result['slug'] = value.slug;
    }

    if (value.isProtected !== asset.isProtected) {
        result['isProtected'] = value.isProtected;
    }

    if (!Types.equals(value.metadata, asset.metadata)) {
        result['metadata'] = value.metadata;
    }

    if (!Types.equals(value.tags, asset.tags)) {
        result['tags'] = value.tags;
    }

    if (Object.keys(result).length === 0) {
        return null;
    }

    return new AnnotateAssetDto(result);
}
