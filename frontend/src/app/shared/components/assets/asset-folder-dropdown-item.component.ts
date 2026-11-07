/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectorRef, Component, computed, input, numberAttribute, output } from '@angular/core';
import { LoaderComponent, StopClickDirective, TranslatePipe } from '@app/framework';
import { AssetsService } from '@app/shared/internal';
import { AssetFolderDropdowNode } from './asset-folder-dropdown.state';

@Component({
    selector: 'sqx-asset-folder-dropdown-item',
    styleUrls: ['./asset-folder-dropdown-item.component.scss'],
    templateUrl: './asset-folder-dropdown-item.component.html',
    imports: [
        LoaderComponent,
        StopClickDirective,
        TranslatePipe,
    ],
})
export class AssetFolderDropdownItemComponent {
    /** The name of the app to load the child folders from. */
    public readonly appName = input.required<string>();

    /** The folder node to render. Its children are loaded on demand. */
    public readonly nodeModel = input.required<AssetFolderDropdowNode>();

    /** The depth of the node in the tree, used for the indentation. */
    public readonly nodeLevel = input(0, { transform: numberAttribute });

    /** Emits when a node is clicked. */
    public readonly selectNode = output<AssetFolderDropdowNode>();

    protected readonly paddingLeft = computed(() => {
        return `${this.nodeLevel()}rem`;
    });

    constructor(
        private readonly assetsService: AssetsService,
        private readonly changeDetector: ChangeDetectorRef,
    ) {
    }

    protected toggle() {
        const nodeModel = this.nodeModel();
        if (nodeModel.isExpanded && nodeModel.isLoaded) {
            this.collapse();
        } else {
            this.expand();
        }
    }

    private collapse() {
        this.nodeModel().isExpanded = false;
    }

    private expand() {
        this.nodeModel().isExpanded = true;

        this.loadChildren();
    }

    private loadChildren() {
        const nodeModel = this.nodeModel();
        if (nodeModel.isLoading || nodeModel.isLoaded) {
            return;
        }

        nodeModel.isLoading = true;

        this.assetsService.getAssetFolders(this.appName(), nodeModel.item.id, 'Items')
            .subscribe({
                next: dto => {
                    if (dto.items.length > 0) {
                        for (const item of dto.items) {
                            if (!nodeModel.children.find(x => x.item.id === item.id)) {
                                nodeModel.children.push({ item, children: [], parent: nodeModel });
                            }
                        }

                        nodeModel.children.sortByString(x => x.item.folderName);
                    }

                    nodeModel.isLoaded = true;
                    this.changeDetector.detectChanges();
                },
                complete: () => {
                    setTimeout(() => {
                        nodeModel.isLoading = false;
                        this.changeDetector.detectChanges();
                    }, 250);
                },
            });
    }
}
