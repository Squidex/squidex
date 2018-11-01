/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { map, startWith } from 'rxjs/operators';

import { Form } from '@app/framework';

export class EditPermissionsForm extends Form<FormArray> {
    constructor() {
        super(new FormArray([]));
    }

    public add() {
        this.form.push(new FormControl(undefined, Validators.required));
    }

    public remove(index: number) {
        this.form.removeAt(index);
    }

    public load(permissions: string[]) {
        while (this.form.controls.length < permissions.length) {
            this.add();
        }

        while (permissions.length > this.form.controls.length) {
            this.form.removeAt(this.form.controls.length - 1);
        }

        super.load(permissions);
    }
}

export class AddRoleForm extends Form<FormGroup> {
    public hasNoName =
        this.form.controls['name'].valueChanges.pipe(startWith(''), map(x => !x || x.length === 0));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: [null,
                [
                    Validators.required
                ]
            ]
        }));
    }
}