/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

declare module 'pikaday/pikaday';
declare module 'progressbar.js';

declare class SquidexEditorWrapper {
    constructor(element: HTMLElement, props: EditorProps);

    update(newProps: Partial<EditorProps>): void;

    setValue(value: string): void;

    setIsDisabled(isDisabled: boolean): void;

    destroy(): void;
}

type Asset = {
    // The alternative text of the image.
    alt?: string;

    // The src to the asset.
    src: string;

    // The mime type.
    type: string;

    // The file name of the asset.
    fileName: string;
};

type Content = {
    // The title of the content.
    id: string;

    // The name of the schema.
    schemaName: string;

    // The title of the content item.
    title: string;
};

type OnSelectAIText = () => Promise<string | undefined | null>;
type OnSelectAssets = () => Promise<Asset[]>;
type OnSelectContents = () => Promise<Content[]>;

type OnChange = (value: string | undefined) => void;

type SquidexEditorMode = 'Html' | 'Markdown';

interface UploadRequest {
    // The file to upload.
    file: File;

    // The upload progress to update.
    progress: (progress: number) => void;
}

interface EditorProps {
    // The mode of the editor.
    mode: SquidexEditorMode;

    // The incoming value.
    value?: string;

    // The base url.
    baseUrl: string;

    // The name to the app.
    appName: string;

    // The class names.
    classNames?: ReadonlyArray<string>;

    // Called when the value has been changed.
    onChange?: OnChange;

    // Called when AI text selected.
    onSelectAIText?: OnSelectAIText;

    // Called when assets are selected.
    onSelectAssets?: OnSelectAssets;

    // Called when content items should be selected.
    onSelectContents?: OnSelectContents;

    // Called when a file needs to be uploaded.
    onUpload?: (images: UploadRequest[]) => DelayedPromiseCreator<Asset>[];

    // True, if disabled.
    isDisabled?: boolean;

    // Indicates whether AI text can be selected.
    canSelectAIText?: boolean;

    // Indicates whether assets can be selected.
    canSelectAssets?: boolean;

    // Indicates whether content items can be selected.
    canSelectContents?: boolean;
}

type DelayedPromiseCreator<T> = (context: unknown) => Promise<T>;