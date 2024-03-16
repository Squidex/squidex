/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, Validators } from '@angular/forms';
import slugify from 'slugify';
import { ExtendedFormGroup, Form, Mutable, TemplatedFormArray, Types } from '@app/framework';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, RenameAssetFolderDto, RenameAssetTagDto } from './../services/assets.service';

export class AnnotateAssetForm extends Form<ExtendedFormGroup, AnnotateAssetDto, AssetDto> {
    public get metadata() {
        return this.form.controls['metadata'] as TemplatedFormArray;
    }

    public get metadataControls(): ReadonlyArray<ExtendedFormGroup> {
        return this.metadata.controls as any;
    }

    constructor() {
        super(new ExtendedFormGroup({
            isProtected: new FormControl(false,
                Validators.nullValidator,
            ),
            fileName: new FormControl('',
                Validators.required,
            ),
            slug: new FormControl('',
                Validators.required,
            ),
            tags: new FormControl([],
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
        const result: Mutable<AnnotateAssetDto> | null = super.submit();

        if (asset && result) {
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
        }

        return result;
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
            name: new FormControl('',
                Validators.required,
            ),
            value: new FormControl('',
                Validators.nullValidator,
            ),
        });
    }
}

export class EditAssetScriptsForm extends Form<ExtendedFormGroup, {}, object> {
    constructor() {
        super(new ExtendedFormGroup({
            annotate: new FormControl('',
                Validators.nullValidator,
            ),
            create: new FormControl('',
                Validators.nullValidator,
            ),
            delete: new FormControl('',
                Validators.nullValidator,
            ),
            move: new FormControl('',
                Validators.nullValidator,
            ),
            update: new FormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}

export class RenameAssetFolderForm extends Form<ExtendedFormGroup, RenameAssetFolderDto, AssetFolderDto> {
    constructor() {
        super(new ExtendedFormGroup({
            folderName: new FormControl('',
                Validators.required,
            ),
        }));
    }
}

export class RenameAssetTagForm extends Form<ExtendedFormGroup, RenameAssetTagDto, RenameAssetTagDto> {
    constructor() {
        super(new ExtendedFormGroup({
            tagName: new FormControl('',
                Validators.required,
            ),
        }));
    }
}
