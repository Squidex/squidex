/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { LongHoverDirective, ModalDirective, ModalModel, ModalPlacementDirective, StatefulComponent, StopClickDirective, Subscriptions, TooltipDirective, TypedSimpleChanges } from '@app/framework';
import { HtmlValue, TableField, TableSettings, Types } from '@app/shared/internal';

interface State {
    wrapping: boolean;
}

@Component({
    standalone: true,
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
    ],
})
export class ContentValueComponent extends StatefulComponent<State> {
    private readonly subscriptions = new Subscriptions();

    @Input({ required: true })
    public value!: any;

    @Input()
    public field?: TableField;

    @Input()
    public fields?: TableSettings;

    public previewModal = new ModalModel();

    public get title() {
        return this.isString && this.isPlain ? this.value : undefined;
    }

    public get isString() {
        return this.field?.rootField?.properties.fieldType === 'String';
    }

    public get isPlain() {
        return !Types.is(this.value, HtmlValue);
    }

    constructor() {
        super({ wrapping: false });
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.fields) {
            this.subscriptions.unsubscribeAll();

            this.subscriptions.add(this.fields?.fieldWrappings
                .subscribe(wrappings => {
                    const wrapping = wrappings[this.field?.name!];

                    this.next({ wrapping });
                }));
        }
    }

    public toggle() {
        if (!this.field) {
            return;
        }

        this.fields?.toggleWrapping(this.field?.name);
    }

    public show() {
        if (!Types.is(this.value, HtmlValue) || !this.value.preview) {
            return;
        }

        this.previewModal.show();
    }

    public hide() {
        this.previewModal.hide();
    }
}
