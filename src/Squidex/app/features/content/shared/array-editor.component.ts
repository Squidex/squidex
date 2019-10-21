/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, Input, QueryList, ViewChildren } from '@angular/core';
import { AbstractControl, FormArray, FormGroup } from '@angular/forms';

import {
    AppLanguageDto,
    EditContentForm,
    RootFieldDto,
    sorted
} from '@app/shared';

import { ArrayItemComponent } from './array-item.component';

@Component({
    selector: 'sqx-array-editor',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArrayEditorComponent {
    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public field: RootFieldDto;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public arrayControl: FormArray;

    @ViewChildren(ArrayItemComponent)
    public children: QueryList<ArrayItemComponent>;

    public itemRemove(index: number) {
        this.form.arrayItemRemove(this.field, this.language, index);
    }

    public itemAdd(value?: FormGroup) {
        this.form.arrayItemInsert(this.field, this.language, value);
    }

    public sort(event: CdkDragDrop<ReadonlyArray<AbstractControl>>) {
        this.sortInternal(sorted(event));
    }

    public collapseAll() {
        this.children.forEach(component => {
            component.collapse();
        });
    }

    public expandAll() {
        this.children.forEach(component => {
            component.expand();
        });
    }

    private reset() {
        this.children.forEach(component => {
            component.reset();
        });
    }

    public move(control: AbstractControl, index: number) {
        let controls = [...this.arrayControl.controls];

        controls.splice(controls.indexOf(control), 1);
        controls.splice(index, 0, control);

        this.sortInternal(controls);
    }

    private sortInternal(controls: ReadonlyArray<AbstractControl>) {
        for (let i = 0; i < controls.length; i++) {
            this.arrayControl.setControl(i, controls[i]);
        }

        this.reset();
    }
}