/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { LongHoverDirective, ModalDirective, ModalModel, ModalPlacementDirective, StopClickDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { HtmlValue, TableField, TableSettings, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-value',
    styleUrls: ['./content-value.component.scss'],
    templateUrl: './content-value.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LongHoverDirective,
        ModalDirective,
        ModalPlacementDirective,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ContentValueComponent {
    /** The value to render, either plain or as HTML. */
    public readonly value = input.required<any>();

    /** The field of the value, to decide how to render it. */
    public readonly field = input<TableField>();

    /** The table settings, to wrap long text. */
    public readonly fields = input<TableSettings>();

    protected readonly previewModal = new ModalModel();

    protected readonly wrapping = computed(() => {
        const fieldName = this.field()?.name;
        if (!fieldName) {
            return false;
        }

        return this.fields()?.fieldWrappings()[fieldName] === true;
    });

    protected readonly isString = computed(() => {
        return this.field()?.rootField?.properties.fieldType === 'String';
    });

    protected readonly isPlain = computed(() => {
        return !Types.is(this.value(), HtmlValue);
    });

    protected readonly title = computed(() => {
        return this.isString() && this.isPlain() ? this.value() : undefined;
    });

    protected toggle() {
        const field = this.field();
        if (!field) {
            return;
        }

        this.fields()?.toggleWrapping(field.name);
    }

    protected show() {
        const value = this.value();
        if (!Types.is(value, HtmlValue) || !value.preview) {
            return;
        }

        this.previewModal.show();
    }

    protected hide() {
        this.previewModal.hide();
    }
}
