/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, Validators } from '@angular/forms';
import { Form, Mutable, TemplatedFormArray, Types, UndefinableFormGroup } from '@app/framework';
import slugify from 'slugify';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, RenameAssetFolderDto, RenameAssetTagDto } from './../services/assets.service';

export class AnnotateAssetForm extends Form<UndefinableFormGroup, AnnotateAssetDto, AssetDto> {
    public get metadata() {
        return this.form.controls['metadata'] as TemplatedFormArray;
    }

    public get metadataControls(): ReadonlyArray<TemplatedFormArray> {
        return this.metadata.controls as any;
    }

    constructor() {
        super(new UndefinableFormGroup({
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
            metadata: new TemplatedFormArray(new MetadataTemplate()),
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
                result.fileName += asset.fileName.substr(index);
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
                fileName = fileName.substr(0, index);
            }

            result.fileName = fileName;
        }

        if (Types.isObject(value.metadata)) {
            result.metadata = [];

            for (const name in value.metadata) {
                if (value.metadata.hasOwnProperty(name)) {
                    const raw = value.metadata[name];

                    let converted = '';

                    if (Types.isString(raw)) {
                        converted = raw;
                    } else if (!Types.isUndefined(raw) && !Types.isNull(raw)) {
                        converted = JSON.stringify(raw);
                    }

                    result.metadata.push({ name, value: converted });
                }
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
                    slug += asset.fileName.substr(index);
                }
            }

            this.form.controls['slug'].setValue(slug);
        }
    }
}

class MetadataTemplate {
    public createControl() {
        return new UndefinableFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
            value: new FormControl(''),
        });
    }
}

export class EditAssetScriptsForm extends Form<UndefinableFormGroup, {}, object> {
    constructor() {
        super(new UndefinableFormGroup({
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

export class RenameAssetFolderForm extends Form<UndefinableFormGroup, RenameAssetFolderDto, AssetFolderDto> {
    constructor() {
        super(new UndefinableFormGroup({
            folderName: new FormControl('',
                Validators.required,
            ),
        }));
    }
}

export class RenameAssetTagForm extends Form<UndefinableFormGroup, RenameAssetTagDto, RenameAssetTagDto> {
    constructor() {
        super(new UndefinableFormGroup({
            tagName: new FormControl('',
                Validators.required,
            ),
        }));
    }
}
