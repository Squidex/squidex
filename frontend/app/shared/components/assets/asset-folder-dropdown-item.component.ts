/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AssetsService } from '@app/shared/internal';
import { AssetFolderDropdowNode } from './asset-folder-dropdown.state';

@Component({
    selector: 'sqx-asset-folder-dropdown-item[appName][node]',
    styleUrls: ['./asset-folder-dropdown-item.component.scss'],
    templateUrl: './asset-folder-dropdown-item.component.html',
})
export class AssetFolderDropdownItemComponent {
    @Input()
    public appName: string;

    @Input()
    public node: AssetFolderDropdowNode;

    @Input()
    public nodeLevel = 0;

    @Output()
    public selectNode = new EventEmitter<AssetFolderDropdowNode>();

    public get style() {
        return { paddingLeft: `${this.nodeLevel}rem` };
    }

    constructor(
        private readonly assetsService: AssetsService,
    ) {
    }

    public toggle() {
        if (this.node.isExpanded && this.node.isLoaded) {
            this.collapse();
        } else {
            this.expand();
        }
    }

    public collapse() {
        this.node.isExpanded = false;
    }

    public expand() {
        this.node.isExpanded = true;

        this.loadChildren();
    }

    public loadChildren() {
        if (this.node.isLoading || this.node.isLoaded) {
            return;
        }

        this.node.isLoading = true;

        this.assetsService.getAssetFolders(this.appName, this.node.item.id, 'Items')
            .subscribe({
                next: dto => {
                    if (dto.items.length > 0) {
                        const parent = this.node;

                        for (const item of dto.items) {
                            if (!parent.children.find(x => x.item.id === item.id)) {
                                parent.children.push({ item, children: [], parent });
                            }
                        }

                        parent.children.sortByString(x => x.item.folderName);
                    }

                    this.node.isLoaded = true;
                },
                complete: () => {
                    setTimeout(() => {
                        this.node.isLoading = false;
                    }, 250);
                },
            });
    }

    public trackByNode(_index: number, node: AssetFolderDropdowNode) {
        return node.item.id;
    }
}
