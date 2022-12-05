declare class EditorPlugin {
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
     * @param {Function} callback: The callback to invoke.
     */
    onInit(callback: () => void): void;

    /**
     * Register an function that is called whenever the value of the content has changed.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: Content value (any).
     */
    onContentChanged(callback: (content: any) => void): void;

    /**
     * Clean the editor SDK.
     */
    clean(): void; 
}

declare class SquidexFormField {
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
     * @param {string} url: The url to navigate to.
     */
    navigate(url: string): void;

    /**
     * Notifies the parent to go to fullscreen mode.
     */
    toggleFullscreen(): void;

    /**
     * Notifies the parent to go to expanded mode.
     */
    toggleExpanded(): void;

    /**
     * Notifies the control container that the value has been changed.
     *
     * @param {any} newValue: The new field value.
     */
    valueChanged(newValue: any): void;

    /**
     * Shows an info alert.
     * 
     * @param {string} text: The info text.
     */
    notifyInfo(text: string): void;

    /**
     * Shows an error alert.
     * 
     * @param {string} text: error info text.
     */
    notifyError(text: string): void;

    /**
     * Shows an confirm dialog.
     * 
     * @param {string} title The title of the dialog.
     * @param {string} text The text of the dialog.
     * @param {function} callback The callback to invoke when the dialog is completed or closed.
     */
    confirm(title: string, text: string, callback: (result: boolean) => void): void;

    /**
     * Shows the dialog to pick assets.
     * 
     * @param {function} callback The callback to invoke when the dialog is completed or closed.
     */
    pickAssets(callback: (assets: any[]) => void): void;

    /**
     * Shows the dialog to pick contents.
     * 
     * @param {string} schemas: The list of schema names.
     * @param {function} callback The callback to invoke when the dialog is completed or closed.
     */
    pickContents(schemas: string[], callback: (assets: any[]) => void): void;

    /**
     * Register an function that is called when the field is initialized.
     * 
     * @param {Function} callback: The callback to invoke.
     */
    onInit(callback: () => void): void;

    /**
     * Register an function that is called when the field is moved.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: New position (number).
     */
    onMoved(callback: (index: number) => void): void;

    /**
     * Register an function that is called whenever the field is disabled or enabled.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: New disabled state (boolean, disabled = true, enabled = false).
     */
    onDisabled(callback: (isDisabled: boolean) => void): void;

    /**
     * Register an function that is called whenever the field language is changed.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: Language code (string).
     */
    onLanguageChanged(callback: (language: string) => void): void;

    /**
     * Register an function that is called whenever the value of the field has changed.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: Field value (any).
     */
    onValueChanged(callback: (value: any) => void): void;

    /**
     * Register an function that is called whenever the value of the content has changed.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: Content value (any).
     */
    onFormValueChanged(callback: (value: any) => void): void;

    /**
     * Register an function that is called whenever the fullscreen mode has changed.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: Fullscreen state (boolean, fullscreen on = true, fullscreen off = false).
     */
    onFullscreen(callback: (isFullscreen: boolean) => void): void;

    /**
     * Register an function that is called whenever the expanded mode has changed.
     *
     * @param {Function} callback: The callback to invoke. Argument 1: Expanded state (boolean, expanded on = true, expanded off = false).
     */
    onExpanded(callback: (isExpanded: boolean) => void): void;

    /**
     * Clean the editor SDK.
     */
    clean(): void;
}