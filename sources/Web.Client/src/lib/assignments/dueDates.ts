/** "yyyy-MM-dd" of the local calendar day (what a date input shows and the API expects). */
export const toDateValue = (date: Date): string => {
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${date.getFullYear()}-${month}-${day}`;
};

/** Today as "yyyy-MM-dd" - the earliest due date the form offers. */
export const todayValue = (): string => toDateValue(new Date());

/**
 * "2026-10-12" -> "12.10.2026" (de) / "10/12/2026" (en). Parsed as a local day, so the date never shifts by a time
 * zone.
 */
export const formatDueDate = (value: string, language: string): string => {
    const [year, month, day] = value.split('-').map(Number);

    return new Date(year, month - 1, day).toLocaleDateString(language, {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
    });
};
