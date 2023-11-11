type PluginOptions = {
    /**
     * Defines the accepted origins for incoming messages.
     */
    acceptedOrigins?: string[];
}

declare class SquidexSidebar {
    /** 
     * The constructor.
     * 
     * @param options: The plugin options.
    */
    constructor(options?: PluginOptions);
    
    /**
     * Get the current context.
     */
    getContext(): any;

    /**
     * Notifies the parent to navigate to the path.
     */
    navigate(url: string): void;

    /**
     * Register an function that is called when the sidebar is initialized.
     *
     * @param callback: The callback to invoke.
     */
    onInit(callback: () => void): void;

    /**
     * Register an function that is called whenever the value of the content has changed.
     *
     * @param callback: The callback to invoke. Argument 1: Content value (any).
     */
    onContentChanged(callback: (content: any) => void): void;

    /**
     * Clean the editor SDK.
     */
    clean(): void; 
}

declare class SquidexWidget {
    /** 
     * The constructor.
     * 
     * @param options: The plugin options.
    */
    constructor(options?: PluginOptions);

    /**
     * Get the current context.
     */
    getContext(): any;

    /**
     * Register an function that is called when the sidebar is initialized.
     *
     * @param callback: The callback to invoke.
     */
    onInit(callback: () => void): void;

    /**
     * Clean the editor SDK.
     */
    clean(): void; 
}


declare class SquidexFormField {
    /** 
     * The constructor.
     * 
     * @param options: The plugin options.
    */
    constructor(options?: PluginOptions);

    /**
     * Get the current value.
     */
     getValue(): any;

    /**
     * Get the current value.
     */
    getContext(): any;

    /**
     * Get the current form value.
     */
    getFormValue(): any;

    /**
     * Get the current field language.
     */
    getLanguage(): string | undefined | null;

    /**
     * Get the current index when the field is an array item. 
     */
    getIndex(): number | undefined | null;

    /**
     * Get the disabled state.
     */
    isDisabled(): boolean;

    /**
     * Get the fullscreen state.
     */
    isFullscreen(): boolean;
    /**
     * Get the expanded state.
     */
    isExpanded(): boolean;

    /**
     * Notifies the control container that the editor has been touched.
     */
    touched(): void;

    /**
     * Notifies the parent to navigate to the path.
     *
     * @param url: The url to navigate to.
     */
    navigate(url: string): void;

    /**
     * Notifies the parent to toggle the fullscreen mode.
     */
    toggleFullscreen(): void;

    /**
     * Notifies the parent to toggle the expanded mode.
     */
    toggleExpanded(): void;

    /**
     * Notifies the control container that the value has been changed.
     *
     * @param newValue: The new field value.
     */
    valueChanged(newValue: any): void;

    /**
     * Shows an info alert.
     * 
     * @param text: The info text.
     */
    notifyInfo(text: string): void;

    /**
     * Shows an error alert.
     * 
     * @param text: error info text.
     */
    notifyError(text: string): void;

    /**
     * Shows an confirm dialog.
     * 
     * @param title The title of the dialog.
     * @param text The text of the dialog.
     * @param callback The callback to invoke when the dialog is completed or closed.
     */
    confirm(title: string, text: string, callback: (result: boolean) => void): void;

    /**
     * Shows the dialog to pick assets.
     * 
     * @param callback The callback to invoke when the dialog is completed or closed.
     */
    pickAssets(callback: (assets: any[]) => void): void;

    /**
     * Shows the dialog to pick contents.
     * 
     * @param schemas: The list of schema names.
     * @param callback The callback to invoke when the dialog is completed or closed.
     * @param query: The initial query that is used in the UI.
     * @param selectedIds: The selected ids to mark them as selected in the content selector dialog.
     */
    pickContents(schemas: string[], callback: (assets: any[]) => void, query?: string, selectedIds?: string[]): void;

    /**
     * Shows a dialog to pick a file.
     */
    pickFile(): void;

    /**
     * Register an function that is called when the field is initialized.
     * 
     * @param callback: The callback to invoke.
     */
    onInit(callback: () => void): void;

    /**
     * Register an function that is called when the field is moved.
     *
     * @param callback: The callback to invoke. Argument 1: New position (number).
     */
    onMoved(callback: (index: number) => void): void;

    /**
     * Register an function that is called whenever the field is disabled or enabled.
     *
     * @param callback: The callback to invoke. Argument 1: New disabled state (boolean, disabled = true, enabled = false).
     */
    onDisabled(callback: (isDisabled: boolean) => void): void;

    /**
     * Register an function that is called whenever the field language is changed.
     *
     * @param callback: The callback to invoke. Argument 1: Language code (string).
     */
    onLanguageChanged(callback: (language: string) => void): void;

    /**
     * Register an function that is called whenever the value of the field has changed.
     *
     * @param callback: The callback to invoke. Argument 1: Field value (any).
     */
    onValueChanged(callback: (value: any) => void): void;

    /**
     * Register an function that is called whenever the value of the content has changed.
     *
     * @param callback: The callback to invoke. Argument 1: Content value (any).
     */
    onFormValueChanged(callback: (value: any) => void): void;

    /**
     * Register an function that is called whenever the fullscreen mode has changed.
     *
     * @param callback: The callback to invoke. Argument 1: Fullscreen state (boolean, fullscreen on = true, fullscreen off = false).
     */
    onFullscreen(callback: (isFullscreen: boolean) => void): void;

    /**
     * Register an function that is called whenever the expanded mode has changed.
     *
     * @param callback: The callback to invoke. Argument 1: Expanded state (boolean, expanded on = true, expanded off = false).
     */
    onExpanded(callback: (isExpanded: boolean) => void): void;

    /**
     * Clean the editor SDK.
     */
    clean(): void;
}