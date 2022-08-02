/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { StatefulComponent } from '@app/framework';
import { HtmlValue, TableField, TableSettings, Types } from '@app/shared/internal';

interface State {
    wrapping: boolean;
}

@Component({
    selector: 'sqx-content-value[value]',
    styleUrls: ['./content-value.component.scss'],
    templateUrl: './content-value.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentValueComponent extends StatefulComponent<State> implements OnChanges {
    @Input()
    public value!: any;

    @Input()
    public field?: TableField;

    @Input()
    public fields?: TableSettings;

    public get title() {
        return this.isString && this.isPlain ? this.value : undefined;
    }

    public get isString() {
        return this.field?.rootField?.properties.fieldType === 'String';
    }

    public get isPlain() {
        return !Types.is(this.value, HtmlValue);
    }

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, { wrapping: false });
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fields']) {
            this.unsubscribeAll();

            this.own(this.fields?.fieldWrappings
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
}
