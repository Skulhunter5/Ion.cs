namespace Ion {

    abstract class Warning {}

    abstract class TypeCheckerWarning : Warning { // TODO: remove or change to a valid thing
        public override string ToString() {
            return "[TypeChecker] WARNING: ";
        }
    }

}