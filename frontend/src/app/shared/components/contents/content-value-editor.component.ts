/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, MathHelper } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-value-editor[field][form]',
    styleUrls: ['./content-value-editor.component.scss'],
    templateUrl: './content-value-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentValueEditorComponent {
    @Input()
    public field!: FieldDto;

    @Input()
    public form!: UntypedFormGroup;

    public readonly uniqueId = MathHelper.guid();
}
