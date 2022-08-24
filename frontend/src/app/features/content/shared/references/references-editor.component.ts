/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { AppLanguageDto, ContentDto, DialogModel, ResolveContents, sorted, StatefulControlComponent, Types } from '@app/shared';

export const SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesEditorComponent), multi: true,
};

interface State {
    // The content items to show.
    contentItems: ReadonlyArray<ContentDto>;

    // True, when width less than 600 pixels.
    isCompact?: boolean;
}

@Component({
    selector: 'sqx-references-editor[formContext][language][languages][schemaIds]',
    styleUrls: ['./references-editor.component.scss'],
    templateUrl: './references-editor.component.html',
    providers: [
        SQX_REFERENCES_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReferencesEditorComponent extends StatefulControlComponent<State, ReadonlyArray<string>> {
    @Input()
    public schemaIds!: ReadonlyArray<string>;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input()
    public formContext!: any;

    @Input()
    public isExpanded = false;

    @Input()
    public allowDuplicates?: boolean | null = true;

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public clonedContent?: ContentDto;

    public contentCreatorDialog = new DialogModel();
    public contentSelectorDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly contentsResolver: ResolveContents,
    ) {
        super(changeDetector, { contentItems: [] });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            if (!Types.equals(obj, this.snapshot.contentItems.map(x => x.id))) {
                const contentIds: string[] = obj;

                this.contentsResolver.resolveMany(contentIds)
                    .subscribe({
                        next: ({ items }) => {
                            this.setContentItems(contentIds.map(id => items.find(c => c.id === id)!).defined());

                            if (this.snapshot.contentItems.length !== contentIds.length) {
                                this.updateValue();
                            }
                        },
                        error: () => {
                            this.setContentItems([]);
                        },
                    });
            }
        } else {
            this.setContentItems([]);
        }
    }

    public setContentItems(contentItems: ReadonlyArray<ContentDto>) {
        this.next({ contentItems });
    }

    public select(contents: ReadonlyArray<ContentDto>) {
        this.setContentItems([...this.snapshot.contentItems, ...contents]);

        if (contents.length > 0) {
            this.updateValue();
        }

        this.contentSelectorDialog.hide();
        this.contentCreatorDialog.hide();
    }

    public remove(content: ContentDto) {
        if (content && !this.snapshot.isDisabled) {
            this.setContentItems(this.snapshot.contentItems.removedBy('id', content));

            this.updateValue();
        }
    }

    public sort(event: CdkDragDrop<ReadonlyArray<ContentDto>>) {
        if (event && !this.snapshot.isDisabled) {
            this.setContentItems(sorted(event));

            this.updateValue();
        }
    }

    public createContent(clone?: ContentDto) {
        this.clonedContent = clone;

        this.contentCreatorDialog.show();
    }

    private updateValue() {
        const ids = this.snapshot.contentItems.map(x => x.id);

        if (ids.length === 0) {
            this.callChange(null);
        } else {
            this.callChange(ids);
        }

        this.callTouched();
    }

    public setCompact(isCompact: boolean) {
        this.next({ isCompact });
    }

    public trackByContent(_index: number, content: ContentDto) {
        return content.id;
    }
}
