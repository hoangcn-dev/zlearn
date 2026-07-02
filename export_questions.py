import os
import json

def unescape_copy_val(val):
    if val == '\\N':
        return None
    # Basic unescaping for COPY format
    val = val.replace('\\t', '\t').replace('\\n', '\n').replace('\\r', '\r').replace('\\\\', '\\')
    return val

def extract_table_rows(lines, table_name):
    rows = []
    in_copy = False
    columns = []
    
    copy_start_prefix = f'COPY public."{table_name}"'
    copy_start_prefix_alt = f'COPY public.{table_name}'
    
    for line in lines:
        stripped = line.strip()
        if not in_copy:
            if stripped.startswith(copy_start_prefix) or stripped.startswith(copy_start_prefix_alt):
                in_copy = True
                start_paren = line.find('(')
                end_paren = line.find(')')
                if start_paren != -1 and end_paren != -1:
                    cols_str = line[start_paren+1:end_paren]
                    columns = [c.strip().strip('"') for c in cols_str.split(',')]
        else:
            if stripped == '\\.':
                in_copy = False
                continue
            parts = line.rstrip('\r\n').split('\t')
            row_dict = {}
            for col, val in zip(columns, parts):
                row_dict[col] = unescape_copy_val(val)
            rows.append(row_dict)
    return rows

def main():
    dump_path = r'c:\Users\HoangCN\Downloads\learn-2026-07-02_135346-dump.sql'
    export_dir = r'd:\projects\ZLearn\exports'
    os.makedirs(export_dir, exist_ok=True)
    
    print("Reading dump file...")
    with open(dump_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    print("Parsing Categories...")
    categories = extract_table_rows(lines, 'Categories')
    category_map = {c['Id']: c for c in categories}
    
    print("Parsing Quizzes...")
    quizzes = extract_table_rows(lines, 'Quizzes')
    quiz_to_category = {q['Id']: q['CategoryId'] for q in quizzes}
    
    print("Parsing Questions...")
    questions = extract_table_rows(lines, 'Questions')
    
    print("Parsing Answers...")
    answers = extract_table_rows(lines, 'Answers')
    
    # Group answers by QuestionId
    answers_by_question = {}
    for ans in answers:
        q_id = ans['QuestionId']
        if q_id not in answers_by_question:
            answers_by_question[q_id] = []
        answers_by_question[q_id].append(ans)
        
    # Group questions by CategoryId
    questions_by_category = {}
    
    for q in questions:
        quiz_id = q['QuizId']
        category_id = quiz_to_category.get(quiz_id, 'unknown_category')
        
        q_answers = answers_by_question.get(q['Id'], [])
        
        # Sort answers by their original key or id if necessary, or keep as is
        # e.g., mapping to format:
        # { "StringContent": "...", "IsCorrectAnswer": ... }
        formatted_answers = []
        for ans in q_answers:
            # Compare Keys as string
            is_correct = (str(ans['Key']) == str(q['CorrectKey'])) if q['CorrectKey'] is not None else False
            formatted_answers.append({
                "StringContent": ans['StringContent'] or "",
                "IsCorrectAnswer": is_correct
            })
            
        formatted_question = {
            "StringContent": q['StringContent'] or "",
            "Explanation": q['Explanation'] or "",
            "Level": 1,
            "CategoryIds": [category_id],
            "Answers": formatted_answers
        }
        
        if category_id not in questions_by_category:
            questions_by_category[category_id] = []
        questions_by_category[category_id].append(formatted_question)
        
    print("Clearing previous exports...")
    if os.path.exists(export_dir):
        for f in os.listdir(export_dir):
            if f.endswith('.json'):
                try:
                    os.remove(os.path.join(export_dir, f))
                except Exception:
                    pass

    print("Writing JSON files per category (chunked by 50)...")
    for cat_id, cat_questions in questions_by_category.items():
        cat_info = category_map.get(cat_id)
        base_name = cat_info['Slug'] if (cat_info and cat_info.get('Slug')) else cat_id
        
        chunk_size = 50
        chunks = [cat_questions[i:i + chunk_size] for i in range(0, len(cat_questions), chunk_size)]
        
        for idx, chunk in enumerate(chunks, 1):
            filename = f"{base_name}-de-{idx}.json"
            file_path = os.path.join(export_dir, filename)
            with open(file_path, 'w', encoding='utf-8') as out_f:
                json.dump(chunk, out_f, ensure_ascii=False, indent=2)
            print(f"Exported {len(chunk)} questions to {file_path}")
        
    print("Done!")

if __name__ == '__main__':
    main()
