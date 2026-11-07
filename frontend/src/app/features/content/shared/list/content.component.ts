
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, inject, input, output, viewChildren } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AppLanguageDto, ConfirmClickDirective, ContentDto, ContentListCellDirective, ContentListCellResizeDirective, ContentListFieldComponent, ContentsState, ContentStatusComponent, DropdownMenuComponent, ExternalLinkDirective, ModalDirective, ModalModel, ModalPlacementDirective, PatchContentForm, SchemaDto, StopClickDirective, TableField, TableSettings, TabRouterlinkDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: '[sqxContent][language][languages][tableFields][schema][tableSettings]',
    styleUrls: ['./content.component.scss'],
    templateUrl: './content.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        ContentListCellDirective,
        ContentListCellResizeDirective,
        ContentListFieldComponent,
        ContentStatusComponent,
        DropdownMenuComponent,
        ExternalLinkDirective,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        StopClickDirective,
        TabRouterlinkDirective,
        TranslatePipe,
    ],
})
export class ContentComponent {
    private readonly changeDetector = inject(ChangeDetectorRef);
    private readonly contentsState = inject(ContentsState);

    private readonly fields = viewChildren(ContentListFieldComponent);

    /** Emits when the user clones the content. */
    public readonly clone = output();

    /** Emits when the user deletes the content. */
    public readonly delete = output();

    /** Emits the new status when the user changes the status of the content. */
    public readonly statusChange = output<string>();

    /** Emits when the user selects or deselects the content. */
    public readonly selectedChange = output<boolean>();

    /** Renders the content as selected. */
    public readonly selected = input(false, { transform: booleanAttribute });

    /** The language to render the values in. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The schema of the content. */
    public readonly schema = input<SchemaDto>();

    /** The fields to render as columns. */
    public readonly tableFields = input.required<ReadonlyArray<TableField>>();

    /** The table settings, used to render nested values. */
    public readonly tableSettings = input.required<TableSettings>();

    /** Shows the action to clone the content. */
    public readonly cloneable = input(false, { transform: booleanAttribute });

    /** The router link to the content. */
    public readonly link = input<any>(null);

    /** The content to render. */
    public readonly content = input.required<ContentDto>({ alias: 'sqxContent' });

    protected readonly dropdown = new ModalModel();

    protected readonly patchAllowed = computed(() => {
        return this.content().canUpdate;
    });

    protected readonly patchForm = computed(() => {
        if (!this.patchAllowed()) {
            return undefined;
        }

        return new PatchContentForm(this.tableFields(), this.language());
    });

    protected readonly isDirty = computed(() => {
        return this.patchForm()?.dirty() === true;
    });

    protected save() {
        const content = this.content();
        const patchForm = this.patchForm();
        if (!content.canUpdate || !patchForm) {
            return;
        }

        const value = patchForm.submit();
        if (!value) {
            return;
        }

        this.contentsState.patch(content, value)
            .subscribe({
                next: () => {
                    patchForm.submitCompleted({ noReset: true });

                    this.changeDetector.detectChanges();
                },
                error: error => {
                    patchForm.submitFailed(error);

                    this.changeDetector.detectChanges();
                },
            });
    }

    protected shouldStop(field: TableField) {
        const isDirty = this.patchForm()?.form.dirty === true;

        if (field.rootField) {
            return isDirty || (field.rootField.isInlineEditable && this.patchAllowed());
        } else {
            return isDirty;
        }
    }

    protected cancel() {
        this.patchForm()?.submitCompleted();

        for (const field of this.fields()) {
            field.reset();
        }
    }
}
