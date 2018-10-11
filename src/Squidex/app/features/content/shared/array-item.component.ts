/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';

import {
    AppLanguageDto,
    EditContentForm,
    FieldDto,
    ImmutableArray,
    RootFieldDto
} from '@app/shared';

@Component({
    selector: 'sqx-array-item',
    styleUrls: ['./array-item.component.scss'],
    templateUrl: './array-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArrayItemComponent implements OnChanges {
    @Output()
    public removing = new EventEmitter();

    @Input()
    public form: EditContentForm;

    @Input()
    public field: RootFieldDto;

    @Input()
    public itemForm: FormGroup;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ImmutableArray<AppLanguageDto>;

    public isInvalid: Observable<boolean>;

    public fieldControls: { field: FieldDto, control: AbstractControl }[];

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['itemForm']) {
            this.isInvalid = this.itemForm.statusChanges.pipe(startWith(this.itemForm.invalid), map(x => this.itemForm.invalid));
        }

        if (changes['itemForm'] || changes['field']) {
            this.fieldControls = this.field.nested.map(field => ({ field, control: this.itemForm.get(field.name)! })).filter(x => !!x.control);
        }
    }
}