namespace Ion {

    class Utils {

        public static string IndentString(string text) {
            string res = "  ";
            int start = 0;
            for(int i = 0; i < text.Length; i++) {
                if(text[i] == '\n' && i < text.Length - 1) {
                    res += text.Substring(start, i - start) + "  ";
                }
            }
            return res;
        }

    }

}