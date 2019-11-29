/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import slugify from 'slugify';

import { Form, Types } from '@app/framework';

import { AssetDto } from './../services/assets.service';

export class AnnotateAssetForm extends Form<FormGroup, { fileName?: string, slug?: string, tags?: ReadonlyArray<string> }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            fileName: ['',
                [
                    Validators.required
                ]
            ],
            slug: ['',
                [
                    Validators.required
                ]
            ],
            tags: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }

    public submit(asset?: AssetDto) {
        const result = super.submit();

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

            if (Types.jsJsonEquals(result.tags, asset.tags)) {
                delete result.tags;
            }

            if (Object.keys(result).length === 0) {
                this.enable();
                return null;
            }
        }

        return result;
    }

    public generateSlug(asset: AssetDto) {
        const fileName = this.form.get('fileName')!.value;

        if (fileName) {
            let slug = slugify(fileName, { lower: true });

            const index = asset.fileName.lastIndexOf('.');

            if (index > 0) {
                slug += asset.fileName.substr(index);
            }

            this.form.get('slug')!.setValue(slug);
        }
    }

    public load(asset: AssetDto) {
        let fileName = asset.fileName;

        const index = fileName.lastIndexOf('.');

        if (index > 0) {
            fileName = fileName.substr(0, index);
        }

        super.load({ fileName, slug: asset.slug, tags: asset.tags });
    }
}

export class AssetFolderForm extends Form<FormGroup, { folderName: string }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            folderName: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }
}