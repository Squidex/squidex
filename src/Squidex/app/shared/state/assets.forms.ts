/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form } from '@app/framework';

import { AssetDto } from './../services/assets.service';

export class RenameAssetForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }

    public submit(asset?: AssetDto) {
        const result = super.submit();

        if (asset) {
            let index = asset.fileName.lastIndexOf('.');
            if (index > 0) {
                result.name += asset.fileName.substr(index);
            }
        }

        return result;
    }

    public load(asset: AssetDto) {
        let name = asset.fileName;

        let index = name.lastIndexOf('.');
        if (index > 0) {
            name = name.substr(0, index);
        }

        super.load({ name });
    }
}