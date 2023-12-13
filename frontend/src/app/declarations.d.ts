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

    setValue(value: EditorValue): void;

    setIsDisabled(isDisabled: boolean): void;

    setAnnotations(annotations?: ReadonlyArray<Annotation> | null): void;

    destroy(): void;
}

type EditorValue = string | Node | undefined | null;

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

type OnAnnotationCreate = (annotation: AnnotationSelection) => void;
type OnAnnotationUpdate = (annotation: ReadonlyArray<Annotation>) => void;
type OnAnnotationFocus = (annotation: ReadonlyArray<string>) => void;
type OnAssetEdit = (id: string) => void;
type OnAssetUpload = (images: UploadRequest[]) => DelayedPromiseCreator<Asset>[];
type OnChange = (value: EditorValue) => void;
type OnContentEdit = (schemaName: string, contentId: string) => void;
type OnSelectAIText = () => Promise<string | undefined | null>;
type OnSelectAssets = () => Promise<Asset[]>;
type OnSelectContents = () => Promise<Content[]>;

type SquidexEditorMode = 'Html' | 'Markdown' | 'State';

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
    value?: EditorValue;

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

    // Called when an asset is to be edited.
    onEditAsset: OnAssetEdit;

    // Called when a content is to be edited.
    onEditContent: OnContentEdit;

    // Called when a file needs to be uploaded.
    onUpload?: OnAssetUpload;

    // Triggered, when an annotation is clicked.
    onAnnotationsFocus?: OnAnnotationFocus;

    // Triggered, when an annotation are updated.
    onAnnotationsUpdate?: OnAnnotationUpdate;

    // Triggered, when an annotation is created.
    onAnnotationCreate?: OnAnnotationCreate;

    // True, if disabled.
    isDisabled?: boolean;

    // Indicates whether AI text can be selected.
    canSelectAIText?: boolean;

    // Indicates whether assets can be selected.
    canSelectAssets?: boolean;

    // Indicates whether content items can be selected.
    canSelectContents?: boolean;

    // Indicates whether annotations can be added.
    canAddAnnotation?: boolean;

    // The annotations.
    annotations?: ReadonlyArray<Annotation> | null;
}

interface AnnotationSelection {
    // The start of the annotation selection.
    from: number;

    // The end of the annotation selection.
    to: number;
}

interface Annotation extends AnnotationSelection {
    // The ID of the annotation.
    id: string;
}

type DelayedPromiseCreator<T> = (context: unknown) => Promise<T>;