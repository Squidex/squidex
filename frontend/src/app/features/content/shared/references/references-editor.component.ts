/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';

import { booleanAttribute, Component, effect, forwardRef, inject, input, model, signal, untracked } from '@angular/core';
import { AppLanguageDto, ContentDto, ContentsColumnsPipe, ContentSelectorComponent, DialogModel, FormValueControlBase, ModalDirective, ResizedDirective, ResolveContents, sorted, TranslatePipe, Types } from '@app/shared';
import { ContentCreatorComponent } from './content-creator.component';
import { ReferenceItemComponent } from './reference-item.component';

@Component({
    selector: 'sqx-references-editor',
    styleUrls: ['./references-editor.component.scss'],
    templateUrl: './references-editor.component.html',
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        forwardRef(() => ContentCreatorComponent),
        ContentsColumnsPipe,
        ContentSelectorComponent,
        ModalDirective,
        ReferenceItemComponent,
        ResizedDirective,
        TranslatePipe,
    ],
    host: {
        '[class.expanded]': 'isExpanded()',
    },
})
export class ReferencesEditorComponent extends FormValueControlBase<ReadonlyArray<string> | null> {
    private readonly contentsResolver = inject(ResolveContents);

    /** The value of the control. */
    public readonly value = model<ReadonlyArray<string> | null>(null);

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

    constructor() {
        super();

        effect(onCleanup => {
            const contentIds = this.value();

            untracked(() => {
                if (!Types.isArrayOfString(contentIds)) {
                    this.setContentItems([]);
                    return;
                }

                if (Types.equals(contentIds, this.contentItems().map(x => x.id))) {
                    return;
                }

                const subscription =
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

                onCleanup(() => subscription.unsubscribe());
            });
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
        if (!content || this.disabled()) {
            return;
        }

        this.setContentItems(this.contentItems().removedBy('id', content));

        this.updateValue();
    }

    protected sort(event: CdkDragDrop<ReadonlyArray<ContentDto>>) {
        if (!event || this.disabled()) {
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
            this.value.set(null);
        } else {
            this.value.set(ids);
        }

        this.markTouched();
    }

    protected setCompact(isCompact: boolean) {
        this.isCompact.set(isCompact);
    }
}
