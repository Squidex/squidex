/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import slugify from 'slugify';
import { ExtendedFormGroup, Form, TemplatedFormArray, Types } from '@app/framework';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, MoveAssetDto, RenameAssetFolderDto, RenameTagDto, UpdateAssetScriptsDto } from '../model';

export class AnnotateAssetForm extends Form<ExtendedFormGroup, AnnotateAssetDto, AssetDto> {
    public get metadata() {
        return this.form.controls['metadata'] as TemplatedFormArray;
    }

    public get metadataControls(): ReadonlyArray<ExtendedFormGroup> {
        return this.metadata.controls as any;
    }

    constructor() {
        super(new ExtendedFormGroup({
            isProtected: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
            fileName: new UntypedFormControl('',
                Validators.required,
            ),
            slug: new UntypedFormControl('',
                Validators.required,
            ),
            tags: new UntypedFormControl([],
                Validators.nullValidator,
            ),
            metadata: new TemplatedFormArray(
                MetadataTemplate.INSTANCE,
            ),
        }));
    }

    public transformSubmit(value: any) {
        const result = { ...value, metadata: {} };

        for (const item of value.metadata) {
            const raw = item.value;

            let parsed = raw;

            if (raw) {
                try {
                    parsed = JSON.parse(raw);
                } catch (ex) {
                    parsed = raw;
                }
            }

            if (parsed === '') {
                parsed = null;
            }

            result.metadata[item.name] = parsed;
        }

        return result;
    }

    public submit(asset?: AssetDto) {
        const result: ({ metadata?: object } & Record<string, any>) | null = super.submit() as any;

        if (!asset || !result) {
            return null;
        }

        const index = asset.fileName.lastIndexOf('.');

        if (index > 0) {
            result.fileName += asset.fileName.substring(index);
        }

        if (result.fileName === asset.fileName) {
            delete result.fileName;
        }

        if (result.slug === asset.slug) {
            delete result.slug;
        }

        if (result.isProtected === asset.isProtected) {
            delete result.isProtected;
        }

        if (Types.equals(result.metadata, asset.metadata)) {
            delete result.metadata;
        }

        if (Types.equals(result.tags, asset.tags)) {
            delete result.tags;
        }

        if (Object.keys(result).length === 0) {
            this.enable();
            return null;
        }

        return new AnnotateAssetDto(result);
    }

    public transformLoad(value: Partial<AssetDto>) {
        const result = { ...value };

        let fileName = value.fileName;

        if (fileName) {
            const index = fileName.lastIndexOf('.');

            if (index > 0) {
                fileName = fileName.substring(0, index);
            }

            result.fileName = fileName;
        }

        if (Types.isObject(value.metadata)) {
            result.metadata = [];

            for (const [name, raw] of Object.entries(value.metadata)) {
                let converted = '';

                if (Types.isString(raw)) {
                    converted = raw;
                } else if (!Types.isUndefined(raw) && !Types.isNull(raw)) {
                    converted = JSON.stringify(raw);
                }

                result.metadata.push({ name, value: converted });
            }
        }

        return result;
    }

    public generateSlug(asset: AssetDto) {
        const fileName = this.form.controls['fileName'].value;

        if (fileName) {
            let slug = slugify(fileName, { lower: true });

            if (asset.fileName) {
                const index = asset.fileName.lastIndexOf('.');

                if (index > 0) {
                    slug += asset.fileName.substring(index);
                }
            }

            this.form.controls['slug'].setValue(slug);
        }
    }
}

class MetadataTemplate {
    public static readonly INSTANCE = new MetadataTemplate();

    public createControl() {
        return new ExtendedFormGroup({
            name: new UntypedFormControl('',
                Validators.required,
            ),
            value: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        });
    }
}

export class EditAssetScriptsForm extends Form<ExtendedFormGroup, UpdateAssetScriptsDto, object> {
    constructor() {
        super(new ExtendedFormGroup({
            query: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            queryPre: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            annotate: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            create: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            delete: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            move: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            update: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        }));
    }

    public transformSubmit(value: any) {
        return new UpdateAssetScriptsDto(value);
    }
}

export class RenameAssetFolderForm extends Form<ExtendedFormGroup, RenameAssetFolderDto, AssetFolderDto> {
    constructor() {
        super(new ExtendedFormGroup({
            folderName: new UntypedFormControl('',
                Validators.required,
            ),
        }));
    }

    protected transformSubmit(value: any) {
        return new RenameAssetFolderDto(value);
    }
}

export class RenameAssetTagForm extends Form<ExtendedFormGroup, RenameTagDto, RenameTagDto> {
    constructor() {
        super(new ExtendedFormGroup({
            tagName: new UntypedFormControl('',
                Validators.required,
            ),
        }));
    }

    protected transformSubmit(value: any) {
        return new RenameTagDto(value);
    }
}

export class MoveAssetForm extends Form<ExtendedFormGroup, MoveAssetDto, AssetDto> {
    constructor() {
        super(new ExtendedFormGroup({
            parentId: new UntypedFormControl('',
                Validators.required,
            ),
        }));
    }

    protected transformSubmit(value: any) {
        return new MoveAssetDto(value);
    }
}
