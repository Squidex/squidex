/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, Mutable, Types } from '@app/framework';
import slugify from 'slugify';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, RenameAssetFolderDto } from './../services/assets.service';

export class AnnotateAssetForm extends Form<FormGroup, AnnotateAssetDto, AssetDto> {
    public get metadata() {
        return this.form.get('metadata')! as FormArray;
    }
    public get metadataControls(): ReadonlyArray<FormGroup> {
        return this.metadata.controls as any;
    }

    constructor(
        private readonly formBuilder: FormBuilder,
    ) {
        super(formBuilder.group({
            isProtected: [false,
                [
                    Validators.nullValidator,
                ],
            ],
            fileName: ['',
                [
                    Validators.required,
                ],
            ],
            slug: ['',
                [
                    Validators.required,
                ],
            ],
            tags: [[],
                [
                    Validators.nullValidator,
                ],
            ],
            metadata: formBuilder.array([]),
        }));
    }

    public addMetadata() {
        this.metadata.push(
            this.formBuilder.group({
                name: ['',
                    [
                        Validators.required,
                    ],
                ],
                value: [''],
            }));
    }

    public removeMetadata(index: number) {
        this.metadata.removeAt(index);
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
            const length = Object.keys(value.metadata).length;

            while (this.metadata.controls.length < length) {
                this.addMetadata();
            }

            while (this.metadata.controls.length > length) {
                this.removeMetadata(this.metadata.controls.length - 1);
            }

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
        const fileName = this.form.get('fileName')!.value;

        if (fileName) {
            let slug = slugify(fileName, { lower: true });

            if (asset.fileName) {
                const index = asset.fileName.lastIndexOf('.');

                if (index > 0) {
                    slug += asset.fileName.substr(index);
                }
            }

            this.form.get('slug')!.setValue(slug);
        }
    }
}

export class EditAssetScriptsForm extends Form<FormGroup, {}, object> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            annotate: '',
            create: '',
            delete: '',
            move: '',
            update: '',
        }));
    }
}

export class RenameAssetFolderForm extends Form<FormGroup, RenameAssetFolderDto, AssetFolderDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            folderName: ['',
                [
                    Validators.required,
                ],
            ],
        }));
    }
}
