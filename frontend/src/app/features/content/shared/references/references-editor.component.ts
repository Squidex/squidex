/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, input, signal } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { AppLanguageDto, ContentDto, ContentsColumnsPipe, ContentSelectorComponent, DialogModel, ModalDirective, ResizedDirective, ResolveContents, SignalControlComponent, sorted, TranslatePipe, Types } from '@app/shared';
import { ContentCreatorComponent } from './content-creator.component';
import { ReferenceItemComponent } from './reference-item.component';

export const SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesEditorComponent), multi: true,
};

@Component({
    selector: 'sqx-references-editor',
    styleUrls: ['./references-editor.component.scss'],
    templateUrl: './references-editor.component.html',
    providers: [
        SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        ContentSelectorComponent,
        ContentsColumnsPipe,
        ModalDirective,
        ReferenceItemComponent,
        ResizedDirective,
        TranslatePipe,
        forwardRef(() => ContentCreatorComponent),
    ],
    host: {
        '[class.expanded]': 'isExpanded()',
    },
})
export class ReferencesEditorComponent extends SignalControlComponent<ReadonlyArray<string>> {
    /** Renders the references in fullscreen mode. */
    public readonly isExpanded = input(false, { transform: booleanAttribute });

    /** The IDs of the schemas to select or create contents from. */
    public readonly schemaIds = input.required<ReadonlyArray<string>>();

    /** The initial full text query of the content selector. */
    public readonly query = input.required<string | undefined>();

    /** The language to render the references in. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Additional context information that is passed to custom editors when creating contents. */
    public readonly formContext = input.required<any>();

    /** Allows to select the same content multiple times. */
    public readonly allowDuplicates = input(true, { transform: booleanAttribute });

    protected readonly contentItems = signal<ReadonlyArray<ContentDto>>([]);
    protected readonly contentCloned = signal<ContentDto | undefined>(undefined);

    protected readonly isCompact = signal(false);

    protected readonly contentCreatorDialog = new DialogModel();
    protected readonly contentSelectorDialog = new DialogModel();

    constructor(
        private readonly contentsResolver: ResolveContents,
    ) {
        super();
    }

    public writeValue(obj: any) {
        if (!Types.isArrayOfString(obj)) {
            this.setContentItems([]);
            return;
        }

        if (Types.equals(obj, this.contentItems().map(x => x.id))) {
            return;
        }

        const contentIds: string[] = obj;

        this.contentsResolver.resolveMany(contentIds)
            .subscribe({
                next: ({ items }) => {
                    this.setContentItems(contentIds.map(id => items.find(c => c.id === id)!).defined());

                    if (this.contentItems().length !== contentIds.length) {
                        this.updateValue();
                    }
                },
                error: () => {
                    this.setContentItems([]);
                },
            });
    }

    private setContentItems(contentItems: ReadonlyArray<ContentDto>) {
        this.contentItems.set(contentItems);
    }

    protected select(contents: ReadonlyArray<ContentDto>) {
        this.setContentItems([...this.contentItems(), ...contents]);

        if (contents.length > 0) {
            this.updateValue();
        }

        this.contentSelectorDialog.hide();
        this.contentCreatorDialog.hide();
    }

    protected remove(content: ContentDto) {
        if (!content || this.isDisabled()) {
            return;
        }

        this.setContentItems(this.contentItems().removedBy('id', content));

        this.updateValue();
    }

    protected sort(event: CdkDragDrop<ReadonlyArray<ContentDto>>) {
        if (!event || this.isDisabled()) {
            return;
        }

        this.setContentItems(sorted(event));

        this.updateValue();
    }

    protected createContent(clone?: ContentDto) {
        this.contentCloned.set(clone);

        this.contentCreatorDialog.show();
    }

    private updateValue() {
        const ids = this.contentItems().map(x => x.id);

        if (ids.length === 0) {
            this.callChange(null);
        } else {
            this.callChange(ids);
        }

        this.callTouched();
    }

    protected setCompact(isCompact: boolean) {
        this.isCompact.set(isCompact);
    }
}
