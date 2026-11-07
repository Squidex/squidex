/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectorRef, Component, inject, linkedSignal, model, signal } from '@angular/core';
import { FormValueControlBase, ModalDirective, ModalModel, ModalPlacementDirective, syncExternalValue, TranslatePipe, Types } from '@app/framework';
import { AppsState, AssetsService, ROOT_ITEM } from '@app/shared/internal';
import { AssetFolderDropdownItemComponent } from './asset-folder-dropdown-item.component';
import { AssetFolderDropdowNode } from './asset-folder-dropdown.state';

@Component({
    selector: 'sqx-asset-folder-dropdown',
    styleUrls: ['./asset-folder-dropdown.component.scss'],
    templateUrl: './asset-folder-dropdown.component.html',
    imports: [
        AssetFolderDropdownItemComponent,
        ModalDirective,
        ModalPlacementDirective,
        TranslatePipe,
    ],
})
export class AssetFolderDropdownComponent extends FormValueControlBase<string | null> {
    private readonly appsState = inject(AppsState);
    private readonly assetsService = inject(AssetsService);
    private readonly changeDetector = inject(ChangeDetectorRef);
    private readonly valueSetter = syncExternalValue(() => this.value, value => this.loadSelection(value));
    private selectedId?: string;

    /** The ID of the selected folder. */
    public readonly value = model<string | null>(null);

    protected readonly selection = linkedSignal(() => this.root);

    protected readonly selectionPath = signal('');

    protected readonly appName = this.appsState.appName;

    protected readonly root: AssetFolderDropdowNode = { item: ROOT_ITEM, children: [], parent: null };

    protected readonly dropdown = new ModalModel();

    private loadSelection(value: string | null) {
        const id = Types.isString(value) ? value : ROOT_ITEM.id;
        if (id === this.selectedId) {
            return;
        }

        const node = this.findNode(this.root, id);
        if (node?.isLoaded) {
            this.select(node, false);
            return;
        }

        this.assetsService.getAssetFolders(this.appName, id, 'PathAndItems')
            .subscribe(dto => {
                let parent = this.root;

                for (const item of dto.path) {
                    let newParent = parent.children.find(x => x.item.id === item.id);

                    if (!newParent) {
                        newParent = { item, children: [], parent };
                        parent.children.push(newParent);
                        parent.children.sortByString(x => x.item.folderName);
                    }

                    parent = newParent;
                }

                if (dto.items.length > 0) {
                    for (const item of dto.items) {
                        if (!parent.children.find(x => x.item.id === item.id)) {
                            parent.children.push({ item, children: [], parent });
                        }
                    }

                    parent.children.sortByString(x => x.item.folderName);
                }

                this.select(parent, false);
            });
    }

    protected select(selected: AssetFolderDropdowNode, emit = true) {
        if (emit && this.disabled()) {
            return;
        }

        this.resetSelected(this.root);

        const path: AssetFolderDropdowNode[] = [];

        let current: AssetFolderDropdowNode | null = selected.parent;

        while (current) {
            path.push(current);

            current.isExpanded = true;
            current = current.parent;
        }

        selected.isSelected = true;

        this.selectedId = selected.item.id;
        this.selection.set(selected);
        this.selectionPath.set(path.filter(x => x.item !== ROOT_ITEM).map(x => x.item.folderName).join('/'));

        if (emit) {
            this.valueSetter(selected.item.id);
            this.markTouched();

            this.dropdown.hide();
        }

        this.changeDetector.markForCheck();
    }

    private resetSelected(node: AssetFolderDropdowNode) {
        node.isSelected = false;

        for (const child of node.children) {
            this.resetSelected(child);
        }
    }

    private findNode(node: AssetFolderDropdowNode, id: string) {
        if (node.item.id === id) {
            return node;
        }

        for (const child of node.children) {
            if (this.findNode(child, id)) {
                return child;
            }
        }

        return undefined;
    }
}
